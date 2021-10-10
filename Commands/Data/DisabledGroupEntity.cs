using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Commands.CommandsStuff;

namespace Commands.Data
{
    public class DisabledGroupEntity
    {
        [NotMapped]
        private static List<DisabledGroupEntity> _entities = new();
        private string _id;

        [Key]
        public string Id
        {
            get => _id ??= Guid.NewGuid().ToString();
            set => _id = value;
        }
        
        public GuildEntity GuildEntity { get; set; }
        public ulong GuildId { get; set; }

        public string Name { get; set; }

        public static implicit operator Group(DisabledGroupEntity ge) => ge.Name;
        
        public override string ToString() => string.Join(':', Id, Name, GuildId);

        public static implicit operator DisabledGroupEntity(string s)
        {
            var cacheEntity = _entities.FirstOrDefault(x => x.ToString() == s);
            if (cacheEntity is not null) return cacheEntity;
            var split = s.Split(':');
            var newEntity = new DisabledGroupEntity
            {
                Id = split[0],
                Name = split[1],
                GuildId = Convert.ToUInt64(split[2])
            };
            _entities.Add(newEntity);
            return newEntity;
        }

        public DisabledGroupEntity()
        {
            _id = Guid.NewGuid().ToString();
        }
    }
}