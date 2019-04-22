using Microsoft.AspNet.SignalR;

namespace TrumguSignalR
{
    /// <summary>
    /// 创建人: 谭福超
    /// 创建时间: 2018年12月25日
    /// 作用: 实现自定义SignalR的ConnectionId
    /// </summary>
    public class UserFactory : IUserIdProvider
    {
        public string GetUserId(IRequest request)
        {
            return string.IsNullOrWhiteSpace(request.QueryString["session_id"]) ? "" : request.QueryString["session_id"];
        }
    }
}
