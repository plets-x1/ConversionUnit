using System.Collections.Generic;
using Plets.Core.ControlAndConversionStructures;
using Plets.Core.ControlStructure;

namespace Plets.Conversion.ConversionUnit {
    public interface ModelingStructureConverter {
        List<GeneralUseStructure> Converter (List<GeneralUseStructure> listModel, StructureType type);
    }
}