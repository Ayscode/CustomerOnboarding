using System;
using System.Collections.Generic;
using System.Text;
using CustomerOnboarding.Entities.common;

namespace CustomerOnboarding.Entities
{
    public partial class State : BaseEntity
    {
        public State()
        {
            Lgas = new HashSet<Lga>();
        }
        public string Name { get; set; }
        public string Description { get; set; }
        public virtual ICollection<Lga> Lgas { get; set; }
    }
}
