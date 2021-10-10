using System;
using System.ComponentModel.DataAnnotations;
using Commands.CommandsStuff;

namespace CommandsTest.Data
{
    public class CommandEntity
    {
        private string _id;

        [Key]
        public string Id
        {
            get => _id ??= Guid.NewGuid().ToString();
            set => _id = value;
        }

        public string Name { get; set; }

        public static implicit operator Command(CommandEntity ce) => ce.Name;

        public CommandEntity()
        {
            Id = _id = Guid.NewGuid().ToString();
        }
    }
}