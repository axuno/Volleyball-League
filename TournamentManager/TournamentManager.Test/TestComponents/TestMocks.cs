using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using Moq.Language.Flow;
using TournamentManager.MultiTenancy;

namespace TournamentManager.Tests.TestComponents
{
    public class TestMocks
    {
        public static Mock<ITenantContext> GetTenantContextMock()
        {
            // recursive mock:
            var orgCtxMock = new Mock<ITenantContext> {DefaultValue = DefaultValue.Mock};
            return orgCtxMock;
        }
        
        public static Mock<DbContext> GetDbContextMock()
        {
            // recursive mock:
            var dbCtxMock = new Mock<DbContext> {DefaultValue = DefaultValue.Mock};
            return dbCtxMock;
        }

        public static Mock<MultiTenancy.AppDb> GetAppDbMock()
        {
            // recursive mock:
            var dbAcc = Mock.Of<MultiTenancy.IDbContext>();
            var appDbMock = new Mock<MultiTenancy.AppDb>(dbAcc) {DefaultValue = DefaultValue.Mock};
            return appDbMock;
        }

        public static Mock<T> GetRepo<T>() where T : class
        {
            var dbAcc = Mock.Of<MultiTenancy.IDbContext>();
            var repoMock = new Mock<T>(dbAcc);
            return repoMock;
        }

    }
}
