using System;
using System.ComponentModel.DataAnnotations;
using Commands.CommandsStuff;

namespace CommandsTest.Data
{
    public class GroupEntity
    {
        private string _id;

        [Key]
        public string Id
        {
            get => _id ??= Guid.NewGuid().ToString();
            set => _id = value;
        }
        
        public string Name { get; set; }

        public static implicit operator Group(GroupEntity ge) => ge.Name;

        public GroupEntity()
        {
            Id = _id = Guid.NewGuid().ToString();
        }
    }
}