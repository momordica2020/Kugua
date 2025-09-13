using System.Numerics;

namespace Kugua.Generators
{
    
    public class KSParser
    {

        public KSParser()
        {

        }




    }



    public class KSNode
    {
        public string ID;
    }

    public class KSNumber : KSNode
    {
        public BigInteger Value;
    }
    public class KSString : KSNode
    {
        public string Value;
    }

    public class KSCommand: KSNode
    {

    }
    public class KSCommandGet : KSCommand
    {
        public string Target;
        public KSNode Result;
    }
    public class KSCommandSet: KSCommand
    {
        public string Target;
        public List<KSNode> Values;
    }

    public class KSCommandCall : KSCommand
    {
        public string Name;
        public Dictionary<string, string> Params;
        public KSNode Result;
    }
}