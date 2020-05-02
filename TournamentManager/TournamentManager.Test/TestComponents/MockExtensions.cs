using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using Moq.Language.Flow;
using TournamentManager.Data;

namespace TournamentManager.Tests.TestComponents
{
    public static class MockExtensions
    {
        public static IReturnsResult<OrganizationContext> SetupAppDb(this Mock<OrganizationContext> orgCtxMock, Mock<AppDb> appDbMock)
        {
            return orgCtxMock.Setup(o => o.AppDb).Returns(appDbMock.Object);
        }
    }
}
