using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

namespace SMUI
{
    /// <summary>
    /// Class "borrowed" from Cinemachine's State Driven camera to extract states information from animator
    /// </summary>
    class StateCollector
    {
        public List<int> States;
        public List<string> StateNames;
        public Dictionary<int, int> StateIndexLookup;
        public Dictionary<int, int> StateParentLookup;

        public void CollectStates(AnimatorController ac, int layerIndex)
        {
            States = new List<int>();
            StateNames = new List<string>();
            StateIndexLookup = new Dictionary<int, int>();
            StateParentLookup = new Dictionary<int, int>();

            StateIndexLookup[0] = States.Count;
            StateNames.Add("(default)");
            States.Add(0);

            if (ac != null && layerIndex >= 0 && layerIndex < ac.layers.Length)
            {
                AnimatorStateMachine fsm = ac.layers[layerIndex].stateMachine;
                string name = fsm.name;
                int hash = Animator.StringToHash(name);
                CollectStatesFromFSM(fsm, name + ".", hash, string.Empty);
            }
        }

        void CollectStatesFromFSM(
            AnimatorStateMachine fsm, string hashPrefix, int parentHash, string displayPrefix)
        {
            var states = fsm.states;
            for (int i = 0; i < states.Length; i++)
            {
                var state = states[i].state;
                int hash = AddState(Animator.StringToHash(hashPrefix + state.name), parentHash, displayPrefix + state.name);

                // Also process clips as pseudo-states, if more than 1 is present.
                // Since they don't have hashes, we can manufacture some.
                var clips = CollectClips(state.motion);
                if (clips.Count > 1)
                {
                    string substatePrefix = displayPrefix + state.name + ".";
                    for (int j = 0; j < clips.Count; ++j)
                        AddState(
                            CreateFakeHash(hash, clips[j]),
                            hash, substatePrefix + clips[j].name);
                }
            }

            var fsmChildren = fsm.stateMachines;
            for (int i = 0; i < fsmChildren.Length; ++i)
            {
                var child = fsmChildren[i];
                string name = hashPrefix + child.stateMachine.name;
                string displayName = displayPrefix + child.stateMachine.name;
                int hash = AddState(Animator.StringToHash(name), parentHash, displayName);
                CollectStatesFromFSM(child.stateMachine, name + ".", hash, displayName + ".");
            }
        }
        
        int CreateFakeHash(int parentHash, AnimationClip clip)
        {
            return Animator.StringToHash(parentHash.ToString() + "_" + clip.name);
        }

        List<AnimationClip> CollectClips(UnityEngine.Motion motion)
        {
            var clips = new List<AnimationClip>();
            var clip = motion as AnimationClip;
            if (clip != null)
                clips.Add(clip);
            var tree = motion as BlendTree;
            if (tree != null)
            {
                var children = tree.children;
                for (int i = 0; i < children.Length; ++i)
                    clips.AddRange(CollectClips(children[i].motion));
            }
            return clips;
        }

        int AddState(int hash, int parentHash, string displayName)
        {
            if (parentHash != 0)
                StateParentLookup[hash] = parentHash;
            StateIndexLookup[hash] = States.Count;
            StateNames.Add(displayName);
            States.Add(hash);
            return hash;
        }
    }
}