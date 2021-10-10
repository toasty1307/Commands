using Commands.CommandsStuff;
using CommandsTest.Commands.Misc;

namespace CommandsTest.Modules
{
    public class GlobalTags : TagsModule
    {
        [Tag]
        public string Test(MessageContext ctx)
        {
            return "test";
        }
        
        [Tag]
        public string Test(InteractionContext ctx)
        {
            return "test";
        }
        
        [Tag]
        public string A(MessageContext ctx)
        {
            return "test";
        }
        
        [Tag]
        public string A(InteractionContext ctx)
        {
            return "test";
        }

        [Tag]
        public string B(MessageContext ctx)
        {
            return "test";
        }
        
        [Tag]
        public string B(InteractionContext ctx)
        {
            return "test";
        }

        [Tag]
        public string C(MessageContext ctx)
        {
            return "test";
        }
        
        [Tag]
        public string C(InteractionContext ctx)
        {
            return "test";
        }

        [Tag]
        public string D(MessageContext ctx)
        {
            return "test";
        }
        
        [Tag]
        public string D(InteractionContext ctx)
        {
            return "test";
        }
        [Tag]
        public string E(MessageContext ctx)
        {
            return "test";
        }
        
        [Tag]
        public string E(InteractionContext ctx)
        {
            return "test";
        }
        [Tag]
        public string F(MessageContext ctx)
        {
            return "test";
        }
        
        [Tag]
        public string F(InteractionContext ctx)
        {
            return "test";
        }
        [Tag]
        public string G(MessageContext ctx)
        {
            return "test";
        }
        
        [Tag]
        public string G(InteractionContext ctx)
        {
            return "test";
        }
        [Tag]
        public string H(MessageContext ctx)
        {
            return "test";
        }
        
        [Tag]
        public string H(InteractionContext ctx)
        {
            return "test";
        }

    }
    
    public class TagsModule {  }
}