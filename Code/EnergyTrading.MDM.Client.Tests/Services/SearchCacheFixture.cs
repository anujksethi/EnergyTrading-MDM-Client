﻿using EnergyTrading.Caching.InMemory;

namespace EnergyTrading.Mdm.Client.Tests.Services
{
    using System.Collections.Generic;
    using System.Runtime.Caching;

    using EnergyTrading.Caching;
    using EnergyTrading.Contracts.Search;
    using EnergyTrading.Mdm.Client.Services;
    using EnergyTrading.Mdm.Client.WebClient;
    using EnergyTrading.Mdm.Contracts;
    using EnergyTrading.Search;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class SearchCacheFixture
    {
        private CachePolicyMdmEntityService<SourceSystem> cacheService;
        private Mock<IMdmEntityService<SourceSystem>> mdmService;
        private Mock<ICacheItemPolicyFactory> policyFactory;
        private PagedWebResponse<IList<SourceSystem>> searchResult;
        private CacheItemPolicy policy;
        
        [SetUp]
        public void Setup()
        {
            this.mdmService = new Mock<IMdmEntityService<SourceSystem>>();
            this.policyFactory = new Mock<ICacheItemPolicyFactory>();
            this.policy = new CacheItemPolicy();
            var inmemoryCacheRepo =new DefaultMdmClientCacheRepository(new InMemoryCacheRepository());
            this.policyFactory.Setup(x => x.CreatePolicy()).Returns(this.policy);

            this.cacheService = new CachePolicyMdmEntityService<SourceSystem>(this.mdmService.Object, this.policyFactory.Object,inmemoryCacheRepo);           
        }

        [Test]
        public void ServiceInvokedOnFirstCall()
        {
            // Arrange
            var search = SearchBuilder.CreateSearch();
            search.AddSearchCriteria(SearchCombinator.And)
                .AddCriteria("A", SearchCondition.Equals, "34", false);

            this.searchResult = new PagedWebResponse<IList<SourceSystem>>();
            this.mdmService.Setup(x => x.Search(search)).Returns(this.searchResult);

            // Act
            var result = this.cacheService.Search(search);

            // Assert
            Assert.AreEqual(this.searchResult, result);
            this.mdmService.Verify(x => x.Search(search), Times.AtMostOnce());
        }

        [Test]
        public void CacheUsedOnSecondCall()
        {
            // Arrange
            var search = SearchBuilder.CreateSearch();
            search.AddSearchCriteria(SearchCombinator.And)
                .AddCriteria("A", SearchCondition.Equals, "34", false);

            this.searchResult = new PagedWebResponse<IList<SourceSystem>>();
            this.mdmService.Setup(x => x.Search(search)).Returns(this.searchResult);

            // Act
            this.cacheService.Search(search);
            var result = this.cacheService.Search(search);

            // Assert
            Assert.AreEqual(this.searchResult, result);
            this.mdmService.Verify(x => x.Search(search), Times.AtMostOnce());
        }

        [Test]
        public void InvalidResponseIsReturned()
        {
            // Arrange
            var search = SearchBuilder.CreateSearch();
            search.AddSearchCriteria(SearchCombinator.And)
                .AddCriteria("A", SearchCondition.Equals, "34", false);

            this.searchResult = new PagedWebResponse<IList<SourceSystem>> { IsValid = false };
            this.mdmService.Setup(x => x.Search(search)).Returns(this.searchResult);

            // Act
            var result = this.cacheService.Search(search);

            // Assert
            Assert.AreEqual(this.searchResult, result);
        }

        [Test]
        public void InvalidResponseIsReturnedAndCached()
        {
            // Arrange
            var search = SearchBuilder.CreateSearch();
            search.AddSearchCriteria(SearchCombinator.And)
                .AddCriteria("A", SearchCondition.Equals, "34", false);

            this.searchResult = new PagedWebResponse<IList<SourceSystem>> { IsValid = false };
            this.mdmService.Setup(x => x.Search(search)).Returns(this.searchResult);

            // Act
            this.cacheService.Search(search);
            var result = this.cacheService.Search(search);

            // Assert
            Assert.AreEqual(this.searchResult, result);
            this.mdmService.Verify(x => x.Search(search), Times.AtMostOnce());
        }
    }
}