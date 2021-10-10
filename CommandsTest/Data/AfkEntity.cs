using System;

namespace CommandsTest.Data
{
    public class AfkEntity
    {
        private string _id;

        public string Id
        {
            get => _id ??= Guid.NewGuid().ToString();
            set => _id = value;
        }
        
        public ulong UserId { get; set; }
        public DateTime AfkSetTime { get; set; }
        public string Message { get; set; }

        public AfkEntity()
        {
            _id = Guid.NewGuid().ToString();
        }
    }
}