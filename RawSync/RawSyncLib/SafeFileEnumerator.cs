using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RawSync
{
    public class SafeFileEnumerator : IEnumerable<string>
    {
        private readonly string _root;
        private readonly string _pattern;
        private readonly IList<Exception> _errors;

        public SafeFileEnumerator(string root, string pattern)
        {
            _root = root;
            _pattern = pattern;
            _errors = new List<Exception>();
        }

        public SafeFileEnumerator(string root, string pattern, IList<Exception> errors)
        {
            _root = root;
            _pattern = pattern;
            _errors = errors;
        }

        public Exception[] Errors()
        {
            return _errors.ToArray();
        }

        class Enumerator : IEnumerator<string>
        {
            IEnumerator<string> _fileEnumerator;
            IEnumerator<string> _directoryEnumerator;
            readonly string _root;
            readonly string _pattern;
            private readonly IList<Exception> _errors;

            public Enumerator(string root, string pattern, IList<Exception> errors)
            {
                _root = root;
                _pattern = pattern;
                _errors = errors;
                _fileEnumerator = Directory.EnumerateFiles(root, pattern).GetEnumerator();
                _directoryEnumerator = Directory.EnumerateDirectories(root).GetEnumerator();
            }

            public string Current
            {
                get
                {
                    if (_fileEnumerator == null) throw new ObjectDisposedException("FileEnumerator");
                    return _fileEnumerator.Current;
                }
            }

            public void Dispose()
            {
                _fileEnumerator?.Dispose();
                _fileEnumerator = null;
                _directoryEnumerator?.Dispose();
                _directoryEnumerator = null;
            }

            object System.Collections.IEnumerator.Current => Current;

            public bool MoveNext()
            {
                if ((_fileEnumerator != null) && (_fileEnumerator.MoveNext()))
                    return true;
                while ((_directoryEnumerator != null) && (_directoryEnumerator.MoveNext()))
                {
                    _fileEnumerator?.Dispose();
                    try
                    {
                        _fileEnumerator = new SafeFileEnumerator(_directoryEnumerator.Current, _pattern, _errors)
                            .GetEnumerator();
                    }
                    catch (Exception ex)
                    {
                        _errors.Add(ex);
                        continue;
                    }

                    if (_fileEnumerator.MoveNext()) return true;
                }

                _fileEnumerator?.Dispose();
                _fileEnumerator = null;
                _directoryEnumerator?.Dispose();
                _directoryEnumerator = null;
                return false;
            }

            public void Reset()
            {
                Dispose();
                _fileEnumerator = Directory.EnumerateFiles(_root, _pattern).GetEnumerator();
                _directoryEnumerator = Directory.EnumerateDirectories(_root).GetEnumerator();
            }
        }

        public IEnumerator<string> GetEnumerator()
        {
            return new Enumerator(_root, _pattern, _errors);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}