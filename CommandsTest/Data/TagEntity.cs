using System;

namespace CommandsTest.Data
{
    public class TagEntity
    {
        private string _id;
        
        public string Id
        {
            get => _id ??= Guid.NewGuid().ToString();
            set => _id = value;
        }
        
        public ulong GuildId { get; set; }
        public GuildEntity GuildEntity { get; set; }
        public ulong ThePersonWhoMadeThisTagUserId { get; set; }
        public string Content { get; set; }
        public string Name { get; set; }

        public TagEntity()
        {
            _id = Guid.NewGuid().ToString();
        }
    }
}