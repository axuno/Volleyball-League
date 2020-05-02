using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using Moq.Language.Flow;
using TournamentManager.Data;

namespace TournamentManager.Tests.TestComponents
{
    public class TestMocks
    {
        public static Mock<OrganizationContext> GetOrganizationContextMock()
        {
            // recursive mock:
            var orgCtxMock = new Mock<OrganizationContext> {DefaultValue = DefaultValue.Mock};
            return orgCtxMock;
        }

        public static Mock<AppDb> GetAppDbMock()
        {
            // recursive mock:
            var dbAcc = Mock.Of<IDbAccess>();
            var appDbMock = new Mock<AppDb>(dbAcc) {DefaultValue = DefaultValue.Mock};
            return appDbMock;
        }

        public static Mock<T> GetRepo<T>() where T : class
        {
            var dbAcc = Mock.Of<IDbAccess>();
            var repoMock = new Mock<T>(dbAcc);
            return repoMock;
        }

    }
}
