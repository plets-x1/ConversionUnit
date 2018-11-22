using System;
using System.Collections.Generic;
using Plets.Modeling.FiniteStateMachine;
using Plets.Modeling.Vfsm;

namespace Plets.Conversion.ConversionUnit {
    public class ConvertToFSM {
        //metodo que chama o metodo converte uma Vfsm em FSM.
        public FiniteStateMachine convertToFSM (Vfsm vfsm) {
            FiniteStateMachine fsm = new FiniteStateMachine ();
            fsm.Name = vfsm.Name;
            convertFSM (vfsm.StateInitial, fsm, vfsm);
            addInitialStateVfsmToFSM (vfsm, fsm);
            return fsm;
        }
        //metodo que procura o estado inicial da Vfsm  e converte as informação para estado da FSM.
        private void addInitialStateVfsmToFSM (Vfsm vfsm, FiniteStateMachine fsm) {
            State stateInitial = new State ();
            TransitionVfsm tVfsm = GetStateInitialVfsm (vfsm.StateInitial, vfsm);
            String name = vfsm.StateInitial.Name;
            foreach (Variable v in tVfsm.ListGuardian) {
                name += v.Condition;
            }
            stateInitial.Name = name;
            stateInitial.Id = vfsm.StateInitial.Id;
            fsm.InitialState = stateInitial;
        }
        //metodo que retorna a transição do estado inicial da Vfsm.
        private TransitionVfsm GetStateInitialVfsm (State state, Vfsm vfsm) {
            foreach (TransitionVfsm t in vfsm.ListTransition) {
                if (t.Source.Name.Equals (state.Name) && ValidaListGuardian (t.ListGuardian, t.ListGuardian, vfsm)) {
                    return t;
                }
            }
            return null;
        }

        //metodo recursivo que converte as Vfsm para FSM.
        private void convertFSM (State state, FiniteStateMachine fsm, Vfsm vfsm) {
            List<TransitionVfsm> ListTranitionVfsm = GetTransition (state, vfsm);
            if (ListTranitionVfsm != null) {
                foreach (TransitionVfsm tVfsm in ListTranitionVfsm) {
                    Transition t = ConvertToTransitionFSM (tVfsm, vfsm);
                    addInfoFSM (t, fsm);
                    convertFSM (tVfsm.Target, fsm, vfsm);
                }

            }
        }
        //adiciona as informação da transição na FSM.
        private void addInfoFSM (Transition t, FiniteStateMachine fsm) {
            fsm.Transitions.Add (t);
            if (!fsm.States.Contains (t.SourceState)) {
                fsm.States.Add (t.SourceState);
            }
            if (!fsm.InputAlphabet.Contains (t.Input)) {
                fsm.InputAlphabet.Add (t.Input);
            }
            if (!fsm.OutputAlphabet.Contains (t.Output)) {
                fsm.OutputAlphabet.Add (t.Output);
            }
        }
        //meetodo que converte uma transição de uma VFSm para uma transição de uma FSM.
        private Transition ConvertToTransitionFSM (TransitionVfsm tVfsm, Vfsm vfsm) {
            Transition t = new Transition ();
            t.Input = tVfsm.Input;
            t.Output = tVfsm.Output;

            State sSource = new State ();
            sSource.Name = Concatenate (vfsm.ListOfGuardian, tVfsm.Source);
            //sSource.Id = t.SourceState.Id;

            UpdateVariableCurrentVfsm (vfsm, tVfsm.ListNewGuardian);
            State Starget = new State ();
            Starget.Name = Concatenate (tVfsm.ListNewGuardian, tVfsm.Target);
            //Starget.Id = t.TargetState.Id;
            t.SourceState = sSource;
            t.TargetState = Starget;

            return t;
        }
        //metodo que concatena sa informações da transição de uma Vfsm.
        private string Concatenate (List<Variable> list, State state) {
            String value = state.Name;
            foreach (Variable v in list) {
                value += v.Condition;
            }
            return value;
        }

        //retorna a transição com o mesmo lista de guardian.
        private List<TransitionVfsm> GetTransition (State state, Vfsm vfsm) {
            List<TransitionVfsm> listTransitionVfsm = new List<TransitionVfsm> ();
            foreach (TransitionVfsm t in vfsm.ListTransition) {
                if (t.Source.Name.Equals (state.Name) && ValidaListGuardian (t.ListGuardian, t.ListNewGuardian, vfsm)) {
                    if (!t.Isvisited) {
                        t.Isvisited = true;
                        listTransitionVfsm.Add (t);
                        // return ;
                    }
                }
            }
            return listTransitionVfsm;
        }
        //valida se todas os guardian são iguais as variaveis corrente da Vfsm.
        private bool ValidaListGuardian (List<Variable> listOfTransition, List<Variable> ListNewGuardian, Vfsm vfsm) {
            try {
                for (int i = 0; i < vfsm.ListOfGuardian.Count; i++) {
                    if (!vfsm.ListOfGuardian[i].Condition.Equals (listOfTransition[i].Condition)) {
                        return false;
                    }
                }
                return true;
            } catch (Exception) {
                return false;
            }
        }
        //atualiza as informções do guardian da Vfsm.
        private void UpdateVariableCurrentVfsm (Vfsm vfsm, List<Variable> listOfTransition) {
            vfsm.ListOfGuardian = new List<Variable> ();
            foreach (Variable v in listOfTransition) {
                vfsm.ListOfGuardian.Add (v);
            }
        }
        /// <summary>
        /// Caso a máquina(FSM) Não estejas totalmente especificado
        /// este método faz que a mpaquina fique totalemte especificado.
        /// </summary>
        /// <param name="fsm"></param>
        public void FullySpecified (FiniteStateMachine fsm) {

            foreach (State s in fsm.States) {
                List<Transition> listTransition = GetTransitionFSM (s, fsm);
                List<String> listInput = ContainsInput (listTransition, fsm.InputAlphabet);
                foreach (String input in listInput) {
                    Transition tError = new Transition (listTransition[0].SourceState, listTransition[0].SourceState, input, "Falha");
                    fsm.AddTransition (tError);
                }

            }

        }

        private List<string> ContainsInput (List<Transition> listTransition, List<string> listInput) {
            List<string> listNewInput = new List<string> ();

            foreach (String input in listInput) {
                bool contem = false;
                foreach (Transition t in listTransition) {
                    if (!contem && t.Input.Equals (input)) {
                        contem = true;

                    }
                }
                if (!contem) {
                    listNewInput.Add (input);
                }
            }
            return listNewInput;
        }

        private List<Transition> GetTransitionFSM (State s, FiniteStateMachine fsm) {
            List<Transition> listTransition = new List<Transition> ();
            foreach (Transition t in fsm.Transitions) {
                if (t.SourceState.Name.Equals (s.Name)) {
                    listTransition.Add (t);
                }
            }
            return listTransition;
        }

    }
}