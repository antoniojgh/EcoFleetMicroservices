using System;
using System.Collections.Generic;
using System.Text;

namespace EcoFleet.BuildingBlocks.Application.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string name, object key) : base($"Entity {name} with ID: {key} was not found.")
        {
        }
    }
}
