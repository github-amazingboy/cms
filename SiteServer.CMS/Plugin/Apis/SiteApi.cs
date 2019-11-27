﻿using System.Collections.Generic;
using System.Threading.Tasks;
using SiteServer.CMS.Context;
using SiteServer.CMS.Core;
using SiteServer.CMS.DataCache;
using SiteServer.Plugin;
using SiteServer.Utils;

namespace SiteServer.CMS.Plugin.Apis
{
    public class SiteApi : ISiteApi
    {
        private SiteApi() { }

        private static SiteApi _instance;
        public static SiteApi Instance => _instance ??= new SiteApi();

        public async Task<int> GetSiteIdByFilePathAsync(string path)
        {
            var site = await PathUtility.GetSiteAsync(path);
            return site?.Id ?? 0;
        }

        public async Task<string> GetSitePathAsync(int siteId)
        {
            if (siteId <= 0) return null;

            var site = await DataProvider.SiteDao.GetAsync(siteId);
            return site == null ? null : PathUtility.GetSitePath(site);
        }

        public async Task<List<int>> GetSiteIdListAsync()
        {
            return await DataProvider.SiteDao.GetSiteIdListAsync();
        }

        public async Task<ISite> GetSiteAsync(int siteId)
        {
            return await DataProvider.SiteDao.GetAsync(siteId);
        }

        //public List<int> GetSiteIdListByAdminName(string adminName)
        //{
        //    var permissionManager = PermissionManager.GetInstance(adminName);
        //    return DataProvider.SiteDao.GetWritingSiteIdList(permissionManager);
        //}

        public async Task<string> GetSitePathAsync(int siteId, string virtualPath)
        {
            var site = await DataProvider.SiteDao.GetAsync(siteId);
            return PathUtility.MapPath(site, virtualPath);
        }

        public async Task<string> GetSiteUrlAsync(int siteId)
        {
            var site = await DataProvider.SiteDao.GetAsync(siteId);
            return PageUtility.GetSiteUrl(site, false);
        }

        public async Task<string> GetSiteUrlAsync(int siteId, string virtualPath)
        {
            var site = await DataProvider.SiteDao.GetAsync(siteId);
            return PageUtility.ParseNavigationUrl(site, virtualPath, false);
        }

        public async Task<string> GetSiteUrlByFilePathAsync(string filePath)
        {
            var siteId = await Instance.GetSiteIdByFilePathAsync(filePath);
            var site = await DataProvider.SiteDao.GetAsync(siteId);
            return await PageUtility.GetSiteUrlByPhysicalPathAsync(site, filePath, false);
        }

        public async Task MoveFilesAsync(int sourceSiteId, int targetSiteId, List<string> relatedUrls)
        {
            if (sourceSiteId == targetSiteId) return;

            var site = await DataProvider.SiteDao.GetAsync(sourceSiteId);
            var targetSite = await DataProvider.SiteDao.GetAsync(targetSiteId);
            if (site == null || targetSite == null) return;

            foreach (var relatedUrl in relatedUrls)
            {
                if (!string.IsNullOrEmpty(relatedUrl) && !PageUtils.IsProtocolUrl(relatedUrl))
                {
                    FileUtility.MoveFile(site, targetSite, relatedUrl);
                }
            }
        }

        public async Task AddWaterMarkAsync(int siteId, string filePath)
        {
            var site = await DataProvider.SiteDao.GetAsync(siteId);
            FileUtility.AddWaterMark(site, filePath);
        }

        public async Task<string> GetUploadFilePathAsync(int siteId, string fileName)
        {
            var site = await DataProvider.SiteDao.GetAsync(siteId);
            var localDirectoryPath = PathUtility.GetUploadDirectoryPath(site, PathUtils.GetExtension(fileName));
            var localFileName = PathUtility.GetUploadFileName(site, fileName);
            return PathUtils.Combine(localDirectoryPath, localFileName);
        }
    }
}
