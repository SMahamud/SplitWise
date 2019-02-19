using System;
using System.Collections;
using System.Text;

namespace Sudoko
{
    // Convenience class for tracking candidates
    public class Candidate:IEnumerable
    {
        bool[] m_values;
        int m_count;
        int m_numCandidates;

        public int Count { get { return m_count; } }


        public Candidate(int numCandidates, bool initialValue)
        {
            m_values = new bool[numCandidates];
            m_count = 0;
            m_numCandidates = numCandidates;

            for (int i = 1; i <= numCandidates; i++)
                this[i] = initialValue;
        }


        public bool this[int key]
        {
            // Allows candidates to be referenced by actual value (i.e. 1-9, rather than 0 - 8)
            get { return m_values[key - 1]; }

            // Automatically tracks the number of candidates
            set
            {
                m_count += (m_values[key - 1] == value) ? 0 : (value == true) ? 1 : -1;
                m_values[key - 1] = value;
            }
        }


        public void SetAll(bool value)
        {
            for (int i = 1; i <= m_numCandidates; i++)
                this[i] = value;
        }


        public override string ToString()
        {
            StringBuilder values = new StringBuilder();
            foreach (int candidate in this)
                values.Append(candidate);
            return values.ToString();
        }

        public IEnumerator GetEnumerator()
        {
            return new CandidateEnumerator(this);
        }

        // Enumerator simplifies iterating over candidates
        private class CandidateEnumerator : IEnumerator
        {
            private int m_position;
            private Candidate m_c;

            public CandidateEnumerator(Candidate c)
            {
                m_c = c;
                m_position = 0;
            }

            // only iterates over valid candidates
            public bool MoveNext()
            {
                ++m_position;
                if (m_position <= m_c.m_numCandidates)
                {
                    if (m_c[m_position] == true)
                        return true;
                    else
                        return MoveNext();
                }
                else
                {
                    return false;
                }
            }

            public void Reset()
            {
                m_position = 0;
            }

            public object Current
            {
                get { return m_position; }
            }

        }
    }
}
