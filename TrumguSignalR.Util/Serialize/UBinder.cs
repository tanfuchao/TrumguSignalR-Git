using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using TrumguSignalR.Model;

namespace TrumguSignalR.Util.Serialize
{
    public class UBinder:SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            if (typeName.EndsWith("t_bf_sys_userObj"))
            {
                return typeof(t_bf_sys_userObj);
            }

            var ass = Assembly.GetExecutingAssembly();
            return ass.GetType(typeName);
        }
    }
}
