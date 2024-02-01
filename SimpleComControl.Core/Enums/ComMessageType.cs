namespace SimpleComControl.Core.Enums
{
    public enum ComMessageType
    {
        Connnect = 1,
        ConnectedMessage,
        Ping,
        PingResponse,
        Disconnect,
        DisconnectedMessage,
        IdentityInfo,
        IdentityInfoResponse,
        SentMessage,
        ReceivedMessage,
        //ResendMessage,
        TestMessage,
        TestResponse,
        HelpMessage,
        HelpResponse,
    }
}
