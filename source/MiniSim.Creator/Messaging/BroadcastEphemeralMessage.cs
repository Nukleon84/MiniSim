using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSim.Creator.Messaging
{
    public class BroadcastEphemeralMessage:BaseMessage
    {
        string _messageText="";
        string _actionText = "";

        Action _callback;


        public string MessageText
        {
            get
            {
                return _messageText;
            }

            set
            {
                _messageText = value;
            }
        }

        public string ActionText { get => _actionText; set => _actionText = value; }
        public Action Callback { get => _callback; set => _callback = value; }

        public BroadcastEphemeralMessage(string text)
        {
            MessageText = text;
        }
        public BroadcastEphemeralMessage(string text, string action, Action callback)
        {
            MessageText = text;
            ActionText = action;
            Callback = callback;
        }

    }
}
