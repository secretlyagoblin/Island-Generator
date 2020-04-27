using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace WanderingRoad.Core.Random
{
    public class LoopingDeck<T>
    {
        private List<Card<T>> _wholeDeck = new List<Card<T>>();
        private List<int> _currentDeck = new List<int>();
        bool _readyForPull = false;
        public bool DebugMode = false;

        public void Add(T item, int number, string name = "")
        {
            var key = _wholeDeck.Count;

            _wholeDeck.Add(new Card<T>(item, number, name));

            _readyForPull = false;

            //for (int i = 0; i < number; i++)
            //{
            //    _currentDeck.Add(key);
            //}
        }

        public void Reset()
        {
            _readyForPull = true;
            _currentDeck.Clear();

            for (int i = 0; i < _wholeDeck.Count; i++)
            {
                for (int u = 0; u < _wholeDeck[i].CardNumber; u++)
                {
                    _currentDeck.Add(i);
                }
            }

            _currentDeck = _currentDeck.OrderBy(x => RNG.NextFloat()).ToList();
        }

        public T Draw()
        {
            if (!_readyForPull)
                Reset();

            if(_currentDeck.Count == 0)
            {
                if (this.DebugMode) Debug.Log($"Shuffled...");

                Reset();
            }

            var index = _currentDeck[0];

            _currentDeck.RemoveAt(0);

            if(this.DebugMode) Debug.Log($"Pulled Card {index}: \"{_wholeDeck[index].Name}\"");

            return _wholeDeck[index].CardValue;
        }
    }


    internal class Card<T>
    {
        public T CardValue;
        public int CardNumber;
        public string Name;

        public Card(T item, int number, string name = "")
        {
            CardValue = item;
            CardNumber = number;
            Name = name;
        }
    }
}

