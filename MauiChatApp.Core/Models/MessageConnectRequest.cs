using MauiChatApp.Core.Bases;

namespace MauiChatApp.Core.Models
{
    public class MessageConnectRequest : MessageRequest
    {
        ///You would more likely have the user or indentity already logged in
        ///But this is a demo workflow, we will allow the endpoint to connect then choose a user to communicate as.
        ///This class will be for the follow up of choosing the indentity.
        ///All Requests must have an identify as User.
        public bool ConnectAs { get; set; }
    }
}
