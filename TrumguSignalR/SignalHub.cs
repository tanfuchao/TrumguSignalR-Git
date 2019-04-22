using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using TrumguSignalR.Log;
using TrumguSignalR.Model.HashModel;
using TrumguSignalR.Service;

namespace TrumguSignalR
{
    public class SignalHub : Hub
    {
        
        private readonly InitService _service = new InitService();
        private static readonly MongoService MongoService = new MongoService();

        /// <summary>
        /// 获取自定义的connectionId
        /// 自定义的connectionId为sessionId
        /// </summary>
        /// <returns></returns>
        public string GetSignalRid()
        {
            return string.IsNullOrWhiteSpace(Context.QueryString["session_id"]) ? "" : Context.QueryString["session_id"];
        }

        /// <summary>
        /// 连接时
        /// </summary>
        /// <returns></returns>
        public override Task OnConnected()
        {
            var signalRid = GetSignalRid();
            Console.WriteLine($"session_id{signalRid}连接成功");
            LogWrite.WriteLogInfo($"-------------------------建立链接----------------------");
            LogWrite.WriteLogInfo($"----sessionId:{signalRid}成功与TrumguSignalR建立链接---");
            try
            {
                _service.AddOnlineCache(signalRid);
                _service.ChangeOnline();
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                LogWrite.WriteLogError(e);
            }
            return base.OnConnected();
        }

        /// <summary>
        /// 断开连接时
        /// </summary>
        /// <param name="stopCalled"></param>
        /// <returns></returns>
        public override Task OnDisconnected(bool stopCalled)
        {
            var signalRid = GetSignalRid();
            Console.WriteLine($"session_id{signalRid}断开连接");
            LogWrite.WriteLogInfo($"-------------------------断开链接----------------------");
            LogWrite.WriteLogInfo($"----sessionId:{signalRid}与TrumguSignalR断开链接-------");
            try
            {
                _service.DelOnlineCache(signalRid);
                _service.ChangeOnline();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                LogWrite.WriteLogError(e);
            }
            return base.OnDisconnected(stopCalled);
        }

        /// <summary>
        /// 重连时
        /// </summary>
        /// <returns></returns>
        public override Task OnReconnected()
        {
            var signalRid = GetSignalRid();
            Console.WriteLine($"session_id{signalRid}重连");
            LogWrite.WriteLogInfo($"-------------------------重新链接----------------------");
            LogWrite.WriteLogInfo($"----sessionId:{signalRid}与TrumguSignalR重新链接-------");
            try
            {
                _service.AddOnlineCache(signalRid);
                _service.ChangeOnline();

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                LogWrite.WriteLogError(e);
            }
            return base.OnReconnected();
        }

        //用户改变自选股时,管理内存
        public void ReloadOwnStockCode(string sessionId)
        {
            Console.WriteLine("自选股发生改变,用户"+sessionId);
            LogWrite.WriteLogInfo($"-------------------------用户修改自选股----------------------");
            LogWrite.WriteLogInfo($"---------------------用户:{sessionId}修改了自选股------------");
            _service.ChangeOwnCodeList(sessionId);
            _service.ChangeOnline();
        }

        //初始化涨跌幅
        public void InitRange()
        {
            LogWrite.WriteLogInfo($"------------用户请求初始化涨跌幅---------------");
            var chgModel = MongoService.GetChgData();
            Clients.Caller.upordown(chgModel);
        }

        //初始化首页自选股信息
        public void InitPortfolio(string sessionId)
        {
            LogWrite.WriteLogInfo($"------------用户:{sessionId}请求初始化首页自选股---------------");
            //获得当前用户所关注的code
            var onlineCodeList = _service.GetOnlineCodeList(sessionId);
            //获得当前用户的自选股信息
            List<HashPortfolio> signalList = MongoService.GetPortfolioList(onlineCodeList);
            Clients.Caller.home_follow(signalList);
        }


    }
}
