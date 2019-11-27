﻿using System;
using System.Collections.Specialized;
using System.Linq;
using System.Web.UI.WebControls;
using SiteServer.CMS.Context;
using SiteServer.Utils;
using SiteServer.CMS.Core;
using SiteServer.CMS.DataCache;
using SiteServer.CMS.Model;
using Content = SiteServer.CMS.Model.Content;

namespace SiteServer.BackgroundPages.Cms
{
    public class ModalCheckState : BasePageCms
    {
        protected override bool IsSinglePage => true;
        public Literal LtlTitle;
        public Literal LtlState;
        public PlaceHolder PhCheckReasons;
        public Repeater RptContents;
        public Button BtnCheck;

        private int _channelId;
        private string _tableName;
        private int _contentId;
        private string _returnUrl;

        public static string GetOpenWindowString(int siteId, Content content, string returnUrl)
        {
            return LayerUtils.GetOpenScript("审核状态",
                PageUtils.GetCmsUrl(siteId, nameof(ModalCheckState), new NameValueCollection
                {
                    {"channelId", content.ChannelId.ToString()},
                    {"contentID", content.Id.ToString()},
                    {"returnUrl", StringUtils.ValueToUrl(returnUrl)}
                }), 560, 500);
        }

        public void Page_Load(object sender, EventArgs e)
        {
            if (IsForbidden) return;

            PageUtils.CheckRequestParameter("siteId", "channelId", "contentID", "returnUrl");

            _channelId = AuthRequest.GetQueryInt("channelId");
            _tableName = ChannelManager.GetTableNameAsync(Site, _channelId).GetAwaiter().GetResult();
            _contentId = AuthRequest.GetQueryInt("contentID");
            _returnUrl = StringUtils.ValueFromUrl(AuthRequest.GetQueryString("returnUrl"));

            var contentInfo = DataProvider.ContentDao.GetAsync(Site, _channelId, _contentId).GetAwaiter().GetResult();

            var (isChecked, checkedLevel) = CheckManager.GetUserCheckLevelAsync(AuthRequest.AdminPermissionsImpl, Site, SiteId).GetAwaiter().GetResult();
            BtnCheck.Visible = CheckManager.IsCheckable(contentInfo.Checked, contentInfo.CheckedLevel, isChecked, checkedLevel);

            LtlTitle.Text = contentInfo.Title;
            LtlState.Text = CheckManager.GetCheckState(Site, contentInfo);

            var checkInfoList = DataProvider.ContentCheckDao.GetCheckListAsync(_tableName, _contentId).GetAwaiter().GetResult();
            if (checkInfoList.Any())
            {
                PhCheckReasons.Visible = true;
                RptContents.DataSource = checkInfoList;
                RptContents.ItemDataBound += RptContents_ItemDataBound;
                RptContents.DataBind();
            }
        }

        private static void RptContents_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            var checkInfo = (ContentCheck)e.Item.DataItem;

            var ltlUserName = (Literal)e.Item.FindControl("ltlUserName");
            var ltlCheckDate = (Literal)e.Item.FindControl("ltlCheckDate");
            var ltlReasons = (Literal)e.Item.FindControl("ltlReasons");

            ltlUserName.Text = DataProvider.AdministratorDao.GetDisplayNameAsync(checkInfo.UserName).GetAwaiter().GetResult();
            ltlCheckDate.Text = DateUtils.GetDateAndTimeString(checkInfo.CheckDate);
            ltlReasons.Text = checkInfo.Reasons;
        }

        public override void Submit_OnClick(object sender, EventArgs e)
        {
            var redirectUrl = ModalContentCheck.GetRedirectUrl(SiteId, _channelId, _contentId, _returnUrl);
            PageUtils.Redirect(redirectUrl);
        }

    }
}
