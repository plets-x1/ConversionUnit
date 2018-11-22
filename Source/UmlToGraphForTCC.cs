using System;
using System.Collections.Generic;
using System.Linq;
using Plets.Core.ControlAndConversionStructures;
using Plets.Core.ControlStructure;
using Plets.Modeling.Graph;
using Plets.Modeling.Uml;

namespace Plets.Conversion.ConversionUnit {
    public class UmlToGraphForTCC : ModelingStructureConverter {
        #region Constructor
        public UmlToGraphForTCC () {

        }
        #endregion

        #region Public Methods
        public List<GeneralUseStructure> Converter (List<GeneralUseStructure> listModel, StructureType type) {
            UmlModel model = listModel.OfType<UmlModel> ().FirstOrDefault ();
            List<DirectedGraph> listGraph = TransformToGraph ((UmlModel) model).ToList ();
            return listGraph.Cast<GeneralUseStructure> ().ToList ();
        }

        public DirectedGraph[] TransformToGraph (UmlModel model) {
            List<DirectedGraph> graphs = new List<DirectedGraph> ();
            foreach (UmlSequenceDiagram sequenceDiagram in model.Diagrams.OfType<UmlSequenceDiagram> ()) {
                DirectedGraph graph = new DirectedGraph ();
                graph = SequenceDiagramToGraph (sequenceDiagram);
                graphs.Add (graph);
            }
            return graphs.ToArray ();
        }

        private DirectedGraph SequenceDiagramToGraph (UmlSequenceDiagram sequenceDiagram) {
            DirectedGraph graph = new DirectedGraph (sequenceDiagram.Name);
            Boolean isRoot = false;

            foreach (UmlMessage message in sequenceDiagram.UmlObjects.OfType<UmlMessage> ()) {
                if (message.ActionType.Equals (2)) {
                    //continue;
                }
                Node sender = (from Node node in graph.Nodes where node.Name.Equals (message.Sender.Name) select node).FirstOrDefault ();
                Node receiver = (from Node node in graph.Nodes where node.Name.Equals (message.Receiver.Name) select node).FirstOrDefault ();
                if (sender == null) {
                    sender = new Node (message.Sender.Name, message.Sender.Id);
                    graph.Nodes.Add (sender);
                }
                if (receiver == null) {
                    receiver = new Node (message.Receiver.Name, message.Receiver.Id);
                    graph.Nodes.Add (receiver);
                }

                try {
                    isRoot = message.Index.Equals (1.0);
                } catch {

                }

                if (isRoot) {
                    graph.RootNode = sender;
                }

                int i = 1;
                Edge edge = new Edge ();
                edge.NodeA = sender;
                edge.NodeB = receiver;

                foreach (KeyValuePair<String, String> tag in message.TaggedValues) {
                    edge.SetTaggedValue (tag.Key, tag.Value);
                }

                edge.SetTaggedValue ("ACTIONTYPE", message.ActionType.ToString ());
                edge.SetTaggedValue ("INDEX", message.Index.ToString ());

                if (!message.ActionType.Equals (2)) {
                    edge.SetTaggedValue ("METHOD", message.Method.Name);
                    edge.SetTaggedValue ("METHODABSTRACT", message.Method.Abstract.ToString ());

                    foreach (UmlMethodParam methodParam in message.Method.Params) {
                        if (!message.ActionType.Equals (0)) {
                            if (methodParam.Kind.Equals ("return")) {
                                edge.SetTaggedValue ("METHODRETURN", methodParam.Type);
                            }
                        } else {
                            edge.SetTaggedValue ("METHODRETURN", "");
                        }
                        edge.SetTaggedValue ("METHODPARAM" + i, methodParam.Name);
                        edge.SetTaggedValue ("METHODPARAM" + i + "KIND", methodParam.Kind);
                        edge.SetTaggedValue ("METHODPARAM" + i + "TYPE", methodParam.Type);
                        i++;
                    }
                    if (!message.ActionType.Equals (2)) {
                        edge.SetTaggedValue ("METHODPARAMQUANTITY", i.ToString ());
                    } else {
                        edge.SetTaggedValue ("METHODPARAMQUANTITY", "0");
                    }
                    edge.SetTaggedValue ("METHODVISIBILITY", message.Method.Visibility);
                } else {
                    edge.SetTaggedValue ("METHOD", "");
                }

                graph.Edges.Add (edge);
            }
            return graph;
        }
        #endregion
    }
}