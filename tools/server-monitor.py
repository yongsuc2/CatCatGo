#!/usr/bin/env python3
"""
CatCatGo 서버 로그 모니터링 도구

사용법:
    python tools/server-monitor.py              # 실시간 모니터링
    python tools/server-monitor.py --summary    # 최근 로그 요약만 출력
    python tools/server-monitor.py --errors     # 에러만 표시
"""

import subprocess
import sys
import os
import re
import time
from collections import defaultdict
from datetime import datetime

# Windows UTF-8 출력 설정
if sys.platform == "win32":
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
    os.environ.setdefault("PYTHONIOENCODING", "utf-8")

# ─── 설정 ───
DOCKER_COMPOSE_FILE = "Server/docker-compose.yml"
SERVICE_NAME = "api"

# 필터링할 노이즈 패턴 (정규식)
NOISE_PATTERNS = [
    r"dbug: Microsoft\.EntityFrameworkCore",
    r"dbug: Microsoft\.Extensions\.Hosting",
    r"dbug: Microsoft\.AspNetCore\.DataProtection",
    r"info: Microsoft\.AspNetCore\.DataProtection",
    r"info: Microsoft\.Hosting\.Lifetime",
    r"dbug: Microsoft\.Extensions\.Http",
    r"dbug: Microsoft\.AspNetCore\.Server\.Kestrel",
    r"info: Microsoft\.AspNetCore\.Routing",
    r"Storing keys in a directory",
    r"User profile is available",
    r"Content root path:",
    r"^\s+(SELECT|INSERT|UPDATE|DELETE|FROM|WHERE|AND|OR|JOIN|LEFT|RIGHT|INNER|CREATE|ALTER|DROP|SET|VALUES|INTO|LIMIT|ORDER|GROUP|HAVING)\b",
    r"^\s+@",  # SQL parameters
    r"^\s+\)",  # SQL closing parens
    r"^\s+objid=",
    r"^\s+deptype",
    r"Compiled query expression",
    r"CommandType='Text'",
    r"Opening connection|Closing connection|Closed connection|Disposing connection|Disposed connection",
    r"Executing DbCommand",
    r"A data reader was disposed",
    r"'AppDbContext' disposed",
    r"An entity of type .* tracked",
]

NOISE_COMPILED = [re.compile(p, re.IGNORECASE) for p in NOISE_PATTERNS]

# API 요청 감지 패턴
REQUEST_START = re.compile(r"Request starting HTTP.*?(GET|POST|PUT|DELETE|PATCH|HEAD)\s+(https?://\S+)")
REQUEST_FINISH = re.compile(r"Request finished.*?(\d{3})\s")
APP_LOG = re.compile(r"(info|warn|fail|crit|dbug):\s+(\S+)")

# ─── 상태 추적 ───
class Monitor:
    def __init__(self):
        self.endpoint_counts = defaultdict(int)       # endpoint → 요청 수
        self.error_counts = defaultdict(int)           # endpoint → 에러 수
        self.status_counts = defaultdict(int)          # status_code → 수
        self.recent_errors = []                        # 최근 에러 목록
        self.window_start = time.time()
        self.window_requests = 0
        self.window_errors = 0
        self.last_method = ""
        self.last_url = ""
        self.spam_detector = defaultdict(list)         # endpoint → [timestamps]
        self.total_lines = 0
        self.filtered_lines = 0

    def is_noise(self, line):
        for pattern in NOISE_COMPILED:
            if pattern.search(line):
                return True
        return False

    def detect_spam(self, endpoint):
        """같은 엔드포인트가 5초 내 10회 이상이면 스팸"""
        now = time.time()
        timestamps = self.spam_detector[endpoint]
        timestamps.append(now)
        # 5초 이상 된 기록 제거
        self.spam_detector[endpoint] = [t for t in timestamps if now - t < 5.0]
        count = len(self.spam_detector[endpoint])
        if count >= 10:
            return count
        return 0

    def process_line(self, line):
        self.total_lines += 1

        # 노이즈 필터링
        if self.is_noise(line):
            self.filtered_lines += 1
            return None

        # Request starting 감지
        m = REQUEST_START.search(line)
        if m:
            self.last_method = m.group(1)
            self.last_url = m.group(2)
            # URL에서 경로만 추출
            path = re.sub(r"https?://[^/]+", "", self.last_url)
            endpoint = f"{self.last_method} {path}"
            self.endpoint_counts[endpoint] += 1
            self.window_requests += 1

            spam_count = self.detect_spam(endpoint)
            if spam_count >= 10 and spam_count % 10 == 0:
                return f"\033[91m[SPAM] SPAM 감지: {endpoint} → 5초 내 {spam_count}회 반복!\033[0m"
            return None

        # Request finished 감지
        m = REQUEST_FINISH.search(line)
        if m:
            status = int(m.group(1))
            self.status_counts[status] += 1

            path = re.sub(r"https?://[^/]+", "", self.last_url)
            endpoint = f"{self.last_method} {path}"

            if status >= 400:
                self.window_errors += 1
                self.error_counts[endpoint] += 1
                error_info = {
                    "time": datetime.now().strftime("%H:%M:%S"),
                    "endpoint": endpoint,
                    "status": status,
                }
                self.recent_errors.append(error_info)
                if len(self.recent_errors) > 50:
                    self.recent_errors.pop(0)

                color = "\033[91m" if status >= 500 else "\033[93m"
                return f"{color}[{error_info['time']}] {endpoint} → {status}\033[0m"

            if status >= 200 and status < 300:
                return f"\033[92m[{datetime.now().strftime('%H:%M:%S')}] {endpoint} → {status}\033[0m"

            return f"[{datetime.now().strftime('%H:%M:%S')}] {endpoint} → {status}"

        # warn/fail/crit 레벨 로그는 표시
        m = APP_LOG.search(line)
        if m:
            level = m.group(1)
            if level in ("warn", "fail", "crit"):
                color = {"warn": "\033[93m", "fail": "\033[91m", "crit": "\033[91;1m"}[level]
                return f"{color}{line.strip()}\033[0m"

        return None

    def print_stats(self):
        now = time.time()
        elapsed = now - self.window_start
        if elapsed < 1:
            return

        rps = self.window_requests / elapsed
        eps = self.window_errors / elapsed

        print(f"\033[90m── 통계: {self.window_requests}req/{elapsed:.0f}s ({rps:.1f}/s) | "
              f"에러: {self.window_errors} ({eps:.1f}/s) | "
              f"필터링: {self.filtered_lines}/{self.total_lines} lines ──\033[0m")

        if self.error_counts:
            print("\033[90m   에러 Top 5:\033[0m")
            sorted_errors = sorted(self.error_counts.items(), key=lambda x: -x[1])[:5]
            for ep, cnt in sorted_errors:
                print(f"\033[90m     {ep}: {cnt}회\033[0m")

        # 윈도우 리셋
        self.window_start = now
        self.window_requests = 0
        self.window_errors = 0

    def print_summary(self):
        print("\n\033[1m=== 서버 로그 요약 ===\033[0m\n")

        print(f"총 라인: {self.total_lines} | 필터링: {self.filtered_lines} | 유효: {self.total_lines - self.filtered_lines}")
        print()

        if self.status_counts:
            print("\033[1m상태 코드 분포:\033[0m")
            for status in sorted(self.status_counts.keys()):
                count = self.status_counts[status]
                color = "\033[92m" if status < 300 else ("\033[93m" if status < 500 else "\033[91m")
                bar = "#" * min(count, 50)
                print(f"  {color}{status}\033[0m: {count:>5}  {color}{bar}\033[0m")
            print()

        if self.endpoint_counts:
            print("\033[1m엔드포인트별 요청 수:\033[0m")
            sorted_eps = sorted(self.endpoint_counts.items(), key=lambda x: -x[1])
            for ep, cnt in sorted_eps[:15]:
                err = self.error_counts.get(ep, 0)
                err_str = f" \033[91m({err} errors)\033[0m" if err > 0 else ""
                print(f"  {cnt:>5}  {ep}{err_str}")
            print()

        if self.recent_errors:
            print(f"\033[1m최근 에러 (마지막 {min(len(self.recent_errors), 10)}건):\033[0m")
            for err in self.recent_errors[-10:]:
                color = "\033[91m" if err["status"] >= 500 else "\033[93m"
                print(f"  {color}[{err['time']}] {err['endpoint']} → {err['status']}\033[0m")
            print()


def run_realtime(errors_only=False):
    monitor = Monitor()
    last_stats = time.time()

    print("\033[1m=== CatCatGo 서버 모니터 ===\033[0m")
    print("\033[90mCtrl+C로 종료 (종료 시 요약 표시)\033[0m\n")

    try:
        proc = subprocess.Popen(
            ["docker", "compose", "-f", DOCKER_COMPOSE_FILE, "logs", "-f", "--tail=100", SERVICE_NAME],
            stdout=subprocess.PIPE,
            stderr=subprocess.STDOUT,
            text=True,
            bufsize=1,
        )

        for line in proc.stdout:
            # docker compose 로그 prefix 제거 (예: "api-1  | ")
            cleaned = re.sub(r"^[\w-]+-\d+\s+\|\s+", "", line)
            result = monitor.process_line(cleaned)

            if result:
                if errors_only and "\033[91m" not in result and "\033[93m" not in result:
                    continue
                print(result)

            # 30초마다 통계 출력
            if time.time() - last_stats > 30:
                monitor.print_stats()
                last_stats = time.time()

    except KeyboardInterrupt:
        print("\n")
        monitor.print_summary()
    except FileNotFoundError:
        print("Error: docker compose를 찾을 수 없습니다.")
        sys.exit(1)


def run_summary():
    monitor = Monitor()

    print("최근 서버 로그를 분석합니다...\n")

    try:
        result = subprocess.run(
            ["docker", "compose", "-f", DOCKER_COMPOSE_FILE, "logs", "--tail=500", SERVICE_NAME],
            capture_output=True, text=True, timeout=10,
        )

        for line in result.stdout.splitlines():
            cleaned = re.sub(r"^[\w-]+-\d+\s+\|\s+", "", line)
            monitor.process_line(cleaned)

        monitor.print_summary()

    except subprocess.TimeoutExpired:
        print("Error: 로그 수집 시간 초과")
    except FileNotFoundError:
        print("Error: docker compose를 찾을 수 없습니다.")


if __name__ == "__main__":
    if "--summary" in sys.argv:
        run_summary()
    elif "--errors" in sys.argv:
        run_realtime(errors_only=True)
    else:
        run_realtime()
