//#define HSI
//#define DFS
//#define Wp

using Plets.Core.ControlStructure;

namespace Plets.Conversion.ConversionUnit {
    public class ModelingStructureConverterFactory {

        public ModelingStructureConverter CreateModelingStructureConverter (StructureType type) {
            switch (type) {
#if HSI
                case StructureType.HSI:
                    return new UmlToFsm ();
#endif
#if DFS
                case StructureType.DFS_TCC:
                    return new UmlToGraphForTCC ();
                case StructureType.DFS:
                    return new UmlToGraph ();
#endif
#if WP
                case StructureType.Wp:
                    return new UmlToFsm ();
#endif
            }
            return null;
        }
    }
}