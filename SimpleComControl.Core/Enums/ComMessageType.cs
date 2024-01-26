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
        IndentityInfo,
        IndentityInfoResponse,
        SentMessage,
        ReceivedMessage,
        //ResendMessage,
        TestMessage,
        TestResponse,
        HelpMessage,
        HelpResponse,
    }
}
