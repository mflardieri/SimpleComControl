using System.Text;

namespace MauiChatApp.Core.Models
{
    public class PacketConverter : IDisposable
    {
        private List<byte> _data;
        private byte[] _dataToArray;
        private int pos;
        public PacketConverter() 
        {
            pos = 0;
            _data = new List<byte>();
            _dataToArray = new byte[1];
        }
        public PacketConverter(byte[] data) 
        {
            pos = 0;
            _data = new List<byte>();
            Write(data);
            _dataToArray = _data.ToArray();
        }
        
        public byte[] ToArray() 
        {
            _dataToArray = _data.ToArray();
            return _dataToArray; 
        }

        #region [ Write Tools ]
        public void Write(byte[] value)
        {
            _data.AddRange(value);
        }
        public void Write(int value)
        {
            _data.AddRange(BitConverter.GetBytes(value));
        }
        //Add Other Write Tools here

        public void Write(bool value) 
        {
            _data.AddRange(BitConverter.GetBytes(value));
        }
        public void Write(string value, Encoding encoding)
        {
            if (value != null && value.Length > 0)
            {
                Write(value.Length);
                _data.AddRange(encoding.GetBytes(value));
            }
            else 
            {
                Write(-1);
            }
        }
        #endregion [Write Tools ]

        #region [ Read Tools ]
        public int ReadInt(bool movePos = true)
        {
            if (_data.Count > pos)
            {
                int rtnVal = BitConverter.ToInt32(_dataToArray, pos);
                if (movePos) { pos += 4; }
                return rtnVal;
            }
            else 
            {
                throw new Exception("Cannot read int!");
            }
        }
        //Add More Read Tools here
        public bool ReadBool(bool movePos = true)
        {
            if (_data.Count > pos)
            {
                bool rtnVal = BitConverter.ToBoolean(_dataToArray, pos);
                if (movePos) { pos += 1; }
                return rtnVal;
            }
            else
            {
                throw new Exception("Cannot read bool!");
            }
        }
        public string ReadString(Encoding encoding, bool movePos = true)
        {
            if (_data.Count > pos)
            {
                string rtnVal = null;
                int length = ReadInt(); //GetHeader
                if(length > 0)
                {
                    rtnVal = encoding.GetString(_dataToArray, pos, length);
                    if (movePos) { pos += length; }
                }
                return rtnVal;
            }
            else
            {
                throw new Exception("Cannot read string!");
            }
        }
        private bool _disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed) 
            { 
                if (disposing)
                {
                    _data = null;
                    _dataToArray = null;
                    pos = 0;
                }
                _disposed = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion [Read Tools ]

    }
}
