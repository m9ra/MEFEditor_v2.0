using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
using System.Reflection.Emit;

namespace TypeSystem.TypeBuilding.ILAnalyzer
{
    static class ILUtilities
    {
        internal static void Print(MethodInfo methodInfo)
        {
            Console.WriteLine(methodInfo);
            foreach (ILInstruction instruciton in ILInstructionLoader.GetInstructions(methodInfo))
            {
                Console.WriteLine(instruciton);
            }
        }

        
        internal static byte[] GetILBytesDynamic(MethodBase methodBase)
        {
            var methodInfo = methodBase as MethodInfo;

            var mtype = methodInfo.GetType();
            var fiOwner = mtype.GetField("m_owner", BindingFlags.Instance | BindingFlags.NonPublic);
            if (fiOwner == null)
                return GetILBytes(methodBase);
            var dynMethod = fiOwner.GetValue(methodInfo) as DynamicMethod;
            

            var ilgen = dynMethod.GetILGenerator();
            var fiBytes = ilgen.GetType().BaseType.GetField("m_ILStream", BindingFlags.Instance | BindingFlags.NonPublic);
            var fiLength = ilgen.GetType().BaseType.GetField("m_length", BindingFlags.Instance | BindingFlags.NonPublic);
            byte[] il = fiBytes.GetValue(ilgen) as byte[];
            int cnt = (int)fiLength.GetValue(ilgen);

            Array.Resize(ref il, cnt);
            return il;
        }

        private static byte[] GetILBytes(MethodBase methodBase)
        {
            var methodBody = methodBase.GetMethodBody();
            if (methodBody != null)
            {
                return methodBody.GetILAsByteArray();
            }
            else
            {
                return new byte[0];
            }
        }

    }
}
