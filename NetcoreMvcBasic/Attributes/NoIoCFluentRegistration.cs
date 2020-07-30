using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetcoreMvcBasic.Attributes
{
    [NoIoCFluentRegistration]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public sealed class NoIoCFluentRegistration : Attribute
    {

    }
}
