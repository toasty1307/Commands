using System.Collections.Generic;
using System.Linq;

namespace Commands.Types
{
    [ArgumentType]
    public class BoolArgumentType : ArgumentType<bool?>
    {
        private readonly List<string>
            _truthy = new[] {"true", "t", "yes", "on", "enable", "enabled", "1", "+"}.ToList();

        private readonly List<string>
            _falsy = new[] {"false", "f", "no", "n", "disable", "disabled", "0", "-"}.ToList();

        public override bool Validate(string argString) =>
            _truthy.Contains(argString.ToLower()) || _falsy.Contains(argString.ToLower());

        public override bool? Parse(string argString) =>
            _truthy.Contains(argString.ToLower());

        public override bool IsEmpty(bool? arg) => false;
    }
}