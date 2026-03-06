using NUnit.Framework;
using CatCatGo.Domain.Meta;

namespace CatCatGo.Tests.Meta
{
    [TestFixture]
    public class AttendanceSystemTests
    {
        private AttendanceSystem _attendance;

        [SetUp]
        public void SetUp()
        {
            _attendance = new AttendanceSystem();
        }

        [Test]
        public void StartsWithAllDaysUnchecked()
        {
            Assert.AreEqual(0, _attendance.GetCurrentDay());
            Assert.IsFalse(_attendance.IsComplete());
        }

        [Test]
        public void CanCheckInOnFirstDay()
        {
            Assert.IsTrue(_attendance.CanCheckIn());
        }

        [Test]
        public void CheckInReturnsOneBased()
        {
            int day = _attendance.CheckIn();
            Assert.AreEqual(1, day);
        }

        [Test]
        public void CheckInMarksDay()
        {
            _attendance.CheckIn();
            Assert.IsTrue(_attendance.CheckedDays[0]);
            Assert.AreEqual(1, _attendance.GetCurrentDay());
        }

        [Test]
        public void CannotCheckInTwiceSameDay()
        {
            _attendance.CheckIn();
            Assert.IsFalse(_attendance.CanCheckIn());

            int secondAttempt = _attendance.CheckIn();
            Assert.AreEqual(-1, secondAttempt);
        }

        [Test]
        public void ResetCycleClearsAllDays()
        {
            _attendance.CheckIn();
            Assert.IsTrue(_attendance.CheckedDays[0]);

            _attendance.ResetCycle();
            Assert.IsFalse(_attendance.CheckedDays[0]);
            Assert.AreEqual(0, _attendance.GetCurrentDay());
            Assert.AreEqual("", _attendance.LastCheckDate);
        }

        [Test]
        public void IsCompleteWhenAllDaysChecked()
        {
            for (int i = 0; i < _attendance.CheckedDays.Length; i++)
            {
                _attendance.CheckedDays[i] = true;
            }
            Assert.IsTrue(_attendance.IsComplete());
        }

        [Test]
        public void GetCurrentDayFindsFirstUnchecked()
        {
            _attendance.CheckedDays[0] = true;
            _attendance.CheckedDays[1] = true;
            _attendance.CheckedDays[2] = false;

            Assert.AreEqual(2, _attendance.GetCurrentDay());
        }

        [Test]
        public void GetCurrentDayReturnsLengthWhenAllChecked()
        {
            for (int i = 0; i < _attendance.CheckedDays.Length; i++)
            {
                _attendance.CheckedDays[i] = true;
            }
            Assert.AreEqual(_attendance.CheckedDays.Length, _attendance.GetCurrentDay());
        }

        [Test]
        public void CannotCheckInWhenAllDaysComplete()
        {
            for (int i = 0; i < _attendance.CheckedDays.Length; i++)
            {
                _attendance.CheckedDays[i] = true;
            }
            _attendance.LastCheckDate = "";

            Assert.IsFalse(_attendance.CanCheckIn());
        }
    }
}
