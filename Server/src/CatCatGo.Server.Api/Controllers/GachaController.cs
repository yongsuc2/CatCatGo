using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatCatGo.Server.Api.Controllers;

[ApiController]
[Route("api/gacha")]
[Authorize]
public class GachaController : ControllerBase
{
    [HttpPost("pull")]
    public ActionResult Pull()
    {
        // TODO: 서버사이드 가챠 실행
        // Domain 어셈블리의 TreasureChest + SeededRandom 재사용
        return Ok(new { Message = "NOT_IMPLEMENTED" });
    }

    [HttpPost("pull10")]
    public ActionResult Pull10()
    {
        // TODO: 서버사이드 10연차 실행
        return Ok(new { Message = "NOT_IMPLEMENTED" });
    }

    [HttpGet("pity")]
    public ActionResult GetPity()
    {
        // TODO: 천장 카운터 조회
        return Ok(new { PityCount = 0, Threshold = 180 });
    }
}
