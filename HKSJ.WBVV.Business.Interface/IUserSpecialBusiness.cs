
using System;
using System.Data;
using System.Collections.Generic;
using HKSJ.WBVV.Business.Interface.Base;
using HKSJ.WBVV.Entity;
using HKSJ.WBVV.Entity.ViewModel;
using HKSJ.WBVV.Entity.ViewModel.Client;
using HKSJ.WBVV.Entity.ViewModel.Manage;
using HKSJ.WBVV.Common.Extender.LinqExtender;

namespace HKSJ.WBVV.Business.Interface
{
    /// <summary>
    /// �û�ר��
    /// </summary>
    public interface IUserSpecialBusiness : IBaseBusiness
    {
        //----------------------------------------------------����ר������ begin------------------------------------------------//
        SpecialView GetUserAlbumsViews();
        SpecialView GetUserAlbumsViews(int vid);
        int AddUserAlbum(string title, string profile, string label,string image);
        UserSpecial GetEditUserAlbum(int albumsId);
        bool EditUserAlbum(int albumId, string title, string remark, string tag);
        bool DeleteUserAlbum(int albumId);
        SpecialDetailView GetUserAlbumVideoViews(int albumId);
        bool DeleteAlbumVideos(int albumId, string videoIds);
        bool AddAlbumVideos(int albumId, string videoIds);
        MyVideoViewResult GetUserVideoViews(int albumId);
        MyVideoViewResult GetUserCollectVideoViews(int albumId);
        bool SetCover(int albumId, int videoId);
        bool UpdateAlbumPic(int albumId, string key);
        bool AddVideo2Albums(int vid, string albumsId);

        //----------------------------------------------------��ҳ-ר��ҳ�� end------------------------------------------------//

        //----------------------------------------------------��ҳ-ר��ҳ�� begin------------------------------------------------//

        SpecialView GetAllAlbumsViews();
        SpecialView GetRecommendAlbumsViews();
        SpecialDetailView GetAlbumVideoViews(int albumId, string isHot);

        //----------------------------------------------------��ҳ-ר��ҳ�� end------------------------------------------------//



        //----------------------------------------------------��̨-��ȡ�Ƽ�ר�� begin------------------------------------------------//

        PageResult GetRecommendAlbumsPageResult(IList<Condtion> condtions, IList<OrderCondtion> orderCondtions);
        bool AddRecommendAlbums(string albumIds, int limitCount = 3);
        bool RemoveRecommendAlbums(string albumIds);
        bool SavaRecommendAlbumsSort(string albumIds, string sortNums);

        //----------------------------------------------------��̨-��ȡ�Ƽ�ר�� end------------------------------------------------//


        //----------------------------------------------------���˿ռ� ר�� begin------------------------------------------------//
        PageResult GetUserAlbumsViewsByOrder(int userId, IList<Condtion> condtions, IList<OrderCondtion> orderCondtions);
        PageResult GetUserAlbumVideosById(int userId, int userSpecialId, IList<Condtion> condtions, IList<OrderCondtion> orderCondtions);

        //----------------------------------------------------���˿ռ� ר��  end------------------------------------------------//

    }
}