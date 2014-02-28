using System;
using System.IO;
using System.Web;

namespace Kiss.Web.Optimization
{
    /// <summary>
    /// The base of anything you want to latch onto the Filter property of a <see cref="System.Web.HttpResponse"/>
    /// object.
    /// </summary>
    /// <remarks>
    /// <p></p>These are generally used with <see cref="HttpModule"/> but you could really use them in
    /// other HttpModules.  This is a general, write-only stream that writes to some underlying stream.  When implementing
    /// a real class, you have to override void Write(byte[], int offset, int count).  Your work will be performed there.
    /// </remarks>
    abstract class HttpOutputFilter : Stream
    {
        private Stream _sink;

        /// <summary>
        /// Subclasses need to call this on contruction to setup the underlying stream
        /// </summary>
        /// <param name="baseStream">The stream we're wrapping up in a filter</param>
        protected HttpOutputFilter(Stream baseStream)
        {
            _sink = baseStream;
        }

        /// <summary>
        /// Allow subclasses access to the underlying stream
        /// </summary>
        protected Stream BaseStream
        {
            get { return _sink; }
        }

        /// <summary>
        /// False.  These are write-only streams
        /// </summary>
        public override bool CanRead
        {
            get { return false; }
        }

        /// <summary>
        /// False.  These are write-only streams
        /// </summary>
        public override bool CanSeek
        {
            get { return false; }
        }

        /// <summary>
        /// True.  You can write to the stream.  May change if you call Close or Dispose
        /// </summary>
        public override bool CanWrite
        {
            get { return _sink.CanWrite; }
        }

        /// <summary>
        /// Not supported.  Throws an exception saying so.
        /// </summary>
        /// <exception cref="NotSupportedException">Thrown.  Always.</exception>
        public override long Length
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Not supported.  Throws an exception saying so.
        /// </summary>
        /// <exception cref="NotSupportedException">Thrown.  Always.</exception>
        public override long Position
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Not supported.  Throws an exception saying so.
        /// </summary>
        /// <exception cref="NotSupportedException">Thrown.  Always.</exception>
        public override long Seek(long offset, System.IO.SeekOrigin direction)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Not supported.  Throws an exception saying so.
        /// </summary>
        /// <exception cref="NotSupportedException">Thrown.  Always.</exception>
        public override void SetLength(long length)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Closes this Filter and the underlying stream.
        /// </summary>
        /// <remarks>
        /// If you override, call up to this method in your implementation.
        /// </remarks>
        public override void Close()
        {
            _sink.Close();
        }

        /// <summary>
        /// Fluses this Filter and the underlying stream.
        /// </summary>
        /// <remarks>
        /// If you override, call up to this method in your implementation.
        /// </remarks>
        public override void Flush()
        {
            if (TransformString != null && _cacheStream.Length > 0)
            {
                _cacheStream = OnTransformString(_cacheStream);

                byte[] buffer = _cacheStream.ToArray();

                _sink.Write(buffer, 0, buffer.Length);

                _cacheStream.SetLength(0);
            }

            _sink.Flush();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (TransformString != null)
                _cacheStream.Write(buffer, 0, buffer.Length);
            else
                _sink.Write(buffer, offset, count);
        }

        /// <summary>
        /// Not supported.
        /// </summary>
        /// <param name="buffer">The buffer to write into.</param>
        /// <param name="offset">The offset on the buffer to write into</param>
        /// <param name="count">The number of bytes to write.  Must be less than buffer.Length</param>
        /// <returns>An int telling you how many bytes were written</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        MemoryStream _cacheStream = new MemoryStream(5000);

        public event Func<string, string> TransformString;
        private MemoryStream OnTransformString(MemoryStream ms)
        {
            if (TransformString == null)
                return ms;

            string content = HttpContext.Current.Response.ContentEncoding.GetString(ms.ToArray());

            content = TransformString(content);

            byte[] buffer = HttpContext.Current.Response.ContentEncoding.GetBytes(content);

            ms = new MemoryStream();
            ms.Write(buffer, 0, buffer.Length);

            return ms;
        }

    }
}
