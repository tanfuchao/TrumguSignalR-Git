using System;
using System.Collections.Generic;
using System.Linq;
using TrumguSignalR.Model.HashModel;
using TrumguSignalR.Model.MongoModel;
using TrumguSignalR.MongoDB;
using TrumguSignalR.Util.Config;

namespace TrumguSignalR.Service
{
    public class MongoService
    {
        private readonly MongoDbHelper _dal = new MongoDbHelper(Config.GetValue("MongoDBConStringEncrypt"), "stock", true, true);

       
        #region stock_chg
        /// <summary>
        /// 获取涨跌幅
        /// </summary>
        /// <returns></returns>
        public stock_chg GetChgData()
        {
            var stockChg = _dal.Find<stock_chg>(m => true).FirstOrDefault();
            return stockChg;
        }
        #endregion

        #region stock_signal_single

        /// <summary>
        /// 根据redis中 code集合(新表)
        /// 组合所有需要推送的自选股信息
        /// </summary>
        /// <returns></returns>
        public List<HashPortfolio> GetPortfolioList(List<string> stockCodeList)
        {
            var list = new List<HashPortfolio>();
            //得到所有秒级数据
            var tikPrices = GetTikData(stockCodeList);
            //只取秒级数据的code值 List<string> codeList
            //var codeList = tikPrices.Select(m=>m.code).ToList();
            //根据codeList在mongodb的信号表中 筛选属于这些code的信号
            //循环秒级数据
            foreach (var tikPrice in tikPrices)
            {
                var stockSignals = _dal.Find<stock_signal_single>(m => m.code==tikPrice.code).Select(m => m.signal).ToList();

                //构建要返回的结构
                var hashPortfolio = new HashPortfolio
                {
                    StockCode = tikPrice.code,
                    Increase = tikPrice.chg,
                    Price = tikPrice.price,
                    //获取当前code的信号
                    Signals = stockSignals
                };
                if (!list.Contains(hashPortfolio))
                {
                    list.Add(hashPortfolio);
                }

            }
            return list;
        }

        /// <summary>
        /// 根据两次时间差 筛选信号
        /// </summary>
        /// <param name="time1">上一次执行时间</param>
        /// <param name="time2">本次执行时间</param>
        /// <returns></returns>
        public List<stock_signal_single> GetStockSignalSingleByTime(DateTime time1, DateTime time2)
        {
            var list = _dal.Find<stock_signal_single>(m => m.time > time1 && m.time <= time2);
            return list;
        }

        //        public int GetStockSignalSingleCount()
        //        {
        //            var date = DateTime.Now.Date;
        //            var count = _dal.Find<stock_signal_single>(m => m.time > date).Count;
        //            return count;
        //        }
        //
        //        public List<stock_signal_single> GetLimitList(int skip, int limit)
        //        {
        //            var date = DateTime.Now.Date;
        //            var list = _dal.Get<stock_signal_single>("stock_signal_single", m => m.time > date, skip, limit, null);
        //            return list;
        //        }
        //
        public List<stock_signal_single> GetAllStockSignalSingle()
        {
            var list = _dal.Find<stock_signal_single>(m => true).ToList();
            return list;
        }

        //构建信号推送结构
        public List<HashPop> CreatePopList(List<stock_signal_single> nowList, List<string> codeList)
        {
            var hashPops = new List<HashPop>();
            foreach (var stockSignalSingle in nowList)
            {
                if (!codeList.Contains(stockSignalSingle.code)) continue;
                var hashPop = new HashPop
                {
                    Code = stockSignalSingle.code,
                    Name = stockSignalSingle.name,
                    Signal = GetStateName(stockSignalSingle.state),
                    Time = stockSignalSingle.time.ToShortTimeString(),
                    Price = stockSignalSingle.price
                };
                hashPops.Add(hashPop);
            }
            return hashPops;
        }



        #endregion

        #region tik_price
        /// <summary>
        /// 根据股票代码得到秒级数据
        /// </summary>
        /// <param name="codeList"></param>
        /// <returns></returns>
        public List<tik_price> GetTikData(List<string> codeList)
        {
            var tikPrices = _dal.Find<tik_price>(m => codeList.Contains(m.code));
            return tikPrices;
        }
        #endregion

        private string GetStateName(string state)
        {
            switch (state)
            {
                case "0.0":
                    return "平仓";
                case "-0.0":
                    return "平仓";
                case "1.0":
                    return "买入";
                case "-1.0":
                    return "卖出";
                default:
                    return "状态未知";

            }
        }


    }
}
