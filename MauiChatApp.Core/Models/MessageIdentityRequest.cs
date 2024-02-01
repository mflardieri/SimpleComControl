using MauiChatApp.Core.Bases;
using MauiChatApp.Core.Enums;

namespace MauiChatApp.Core.Models
{
    public class MessageIdentityRequest : MessageRequest
    {
        //For Indentication this request only cares about Ids.
        //Certain Inquiry Types will care if the Identity on the Request is set or not.
        public ChatIdentity Current { get; set; }
        public MessageIdentityInquiryType InquiryType { get; set; }
    }
}
