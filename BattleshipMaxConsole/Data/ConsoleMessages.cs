using System;
using System.Collections.Generic;
using System.Text;

namespace BattleshipMaxConsole.Data
{
    public class ConsoleMessages
    {
        public List<Message> PrintedMessages { get; set; }

        public void AddMessage(string _message, bool _isBlue)
        {
            if (this.PrintedMessages == null)
            {
                this.PrintedMessages = new List<Message>();
            }

            if (this.PrintedMessages.Count == 12)
            {
                this.PrintedMessages.RemoveAt(0);
                this.PrintedMessages.Add(new Message() { Text = _message, IsBlue = _isBlue });
            }
            else
            {
                this.PrintedMessages.Add(new Message() { Text = _message, IsBlue = _isBlue });
            }
        }
    }
}
