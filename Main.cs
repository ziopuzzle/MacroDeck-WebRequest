using SuchByte.MacroDeck.Plugins;
using ziopuzzle.WebRequest.Action;

namespace ziopuzzle.WebRequest
{
    public class Main : MacroDeckPlugin
    {
        public override bool CanConfigure => false;

        public static Main Instance { get; private set; }

        public Main()
        {
            Instance = this;
        }
        
        public override void Enable()
        {
            this.Actions = [new WebRequestAction()];
        }
    }
}