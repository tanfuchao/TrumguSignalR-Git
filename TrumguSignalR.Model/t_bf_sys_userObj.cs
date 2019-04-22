using System;

namespace TrumguSignalR.Model
{
    [Serializable]
    public class t_bf_sys_userObj
    {
        /// <summary>
        /// ID
        /// </summary>
        public int? id { get; set; }


        /// <summary>
        /// 用户名
        /// </summary>
        public string name { get; set; }


        /// <summary>
        /// 登录账号
        /// </summary>
        public string userid { get; set; }


        /// <summary>
        /// 用户密码
        /// </summary>
        public string password { get; set; }


        /// <summary>
        /// 用户状态
        /// </summary>
        public int? status { get; set; }


        /// <summary>
        /// 最近登录
        /// </summary>
        public DateTime? lastlogin { get; set; }


        /// <summary>
        /// 登录IP
        /// </summary>
        public string loginip { get; set; }


        /// <summary>
        /// 登录次数
        /// </summary>
        public int? loginsum { get; set; }


        /// <summary>
        /// 可否登录
        /// </summary>
        public int? islogin { get; set; }


        /// <summary>
        /// 颜色方案
        /// </summary>
        
        public string color { get; set; }


        /// <summary>
        /// MAC地址
        /// </summary>
        public string macaddr { get; set; }


        /// <summary>
        /// 增加登录次数的值
        /// </summary>
        public int? _AddLoginNum { get; set; }

        /// <summary>
        /// 新密码->修改密码时使用
        /// </summary>
        public string newPassword { get; set; }

        
        public DateTime? expiretime { get; set; }

        
        public bool? clearExpiretimeTime { get; set; }

        
        public string person_liable { get; set; }

        /// <summary>
        /// 批量添加用户时使用
        /// </summary>
        
        public int? roleid { get; set; }

        
        public int? startlist { get; set; }

        
        public int? endlist { get; set; }

        /// <summary>
        /// 电话号码
        /// </summary>
        
        public string telephone { get; set; }

        
        public string company_name { get; set; }

        
        public string hpcompany_id { get; set; }//私募公司id

        
        public bool? clear_hpcompany_id { get; set; }

        private int? _parents_id { get; set; }
        
        public int? parents_id { get { return _parents_id; } set { _parents_id = value; } }

        
        private string _parents_name { get; set; }

        
        public string parents_name { get; set; }

        
        public int? ispass { get; set; }//是否开通的状态
        
        public string special_id { get; set; }//调用时候的身份标识
        
        public string token { get; set; }//登陆token;
        
        public string regis_code { get; set; }
        
        public string cn_name { get; set; }// c.cn_name

        
        public int? isagree { get; set; }//是否同意投顾池的签约

        private DateTime? _create_time = null;
        
        public DateTime? create_time { get { return _create_time; } set { _create_time = value; } }

        private string _create_user_name = null;
        
        public string create_user_name { get { return _create_user_name; } set { _create_user_name = value; } }

        private int? _create_user_id = null;
        
        public int? create_user_id { get { return _create_user_id; } set { _create_user_id = value; } }

        private int? _is_person_liable = null;
        
        public int? is_person_liable
        {
            get { return _is_person_liable; }
            set { _is_person_liable = value; }
        }

        private int? _person_liable_id = null;
        
        public int? person_liable_id { get { return _person_liable_id; } set { _person_liable_id = value; } }

        /// <summary>
        /// 清空负责人
        /// </summary>
        
        public bool? clear_person { get; set; }
        private string _mailbox = null;
        
        public string mailbox { get { return _mailbox; } set { _mailbox = value; } }
        private string _department = null;
        
        public string department { get { return _department; } set { _department = value; } }

        
        public string maxsubuserid { get; set; }

        
        public int? iscompany_show { get; set; }//消息是否显示公司名称

    }
}
