using System;
using System.IO;

namespace AdvancedInteractionSystem
{
    internal class WaveStream : Stream
    {
        private BinaryReader reader;
        private byte[] header;
        private int headerOffset;
        private int _volume;
        private const int MaxVolume = 100;

        public override bool CanSeek => false;

        public override bool CanRead => !this.IsClosed;

        public override bool CanWrite => false;

        private bool IsClosed => this.reader == null;

        public override long Position
        {
            get
            {
                this.CheckDisposed();
                throw new NotSupportedException();
            }
            set
            {
                this.CheckDisposed();
                throw new NotSupportedException();
            }
        }

        public override long Length
        {
            get
            {
                this.CheckDisposed();
                throw new NotSupportedException();
            }
        }

        public int Volume
        {
            get
            {
                this.CheckDisposed();
                return this._volume;
            }
            set
            {
                this.CheckDisposed();
                this._volume = value >= 0 && 100 >= value ? value : throw new ArgumentOutOfRangeException(nameof(Volume), (object)value, string.Format("0から{0}の範囲の値を指定してください", (object)100));
            }
        }

        public WaveStream(Stream baseStream)
        {
            this.headerOffset = 0;
            this._volume = 100;
            if (baseStream == null)
                throw new ArgumentNullException(nameof(baseStream));
            this.reader = baseStream.CanRead ? new BinaryReader(baseStream) : throw new ArgumentException("読み込み可能なストリームを指定してください", nameof(baseStream));
            this.ReadHeader();
        }

        public override void Close()
        {
            if (this.reader == null)
                return;
            this.reader.Close();
            this.reader = (BinaryReader)null;
        }

        private void ReadHeader()
        {
            using (MemoryStream output = new MemoryStream())
            {
                BinaryWriter binaryWriter = new BinaryWriter((Stream)output);
                byte[] buffer1 = this.reader.ReadBytes(12);
                binaryWriter.Write(buffer1);
                while (true)
                {
                    byte[] buffer2 = this.reader.ReadBytes(8);
                    binaryWriter.Write(buffer2);
                    int int32_1 = BitConverter.ToInt32(buffer2, 0);
                    int int32_2 = BitConverter.ToInt32(buffer2, 4);
                    if (int32_1 != 1635017060)
                        binaryWriter.Write(this.reader.ReadBytes(int32_2));
                    else
                        break;
                }
                binaryWriter.Close();
                this.header = output.ToArray();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            this.CheckDisposed();
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset), (object)offset, "0以上の値を指定してください");
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), (object)count, "0以上の値を指定してください");
            if (checked(buffer.Length - count) < offset)
                throw new ArgumentException("配列の範囲を超えてアクセスしようとしました", nameof(offset));
            int num1;
            if (this.header == null)
            {
                int num2 = count / 2;
                int count1 = checked(num2 * 2);
                int num3 = this.reader.Read(buffer, offset, count1);
                if (num3 == 0)
                {
                    num1 = 0;
                }
                else
                {
                    int num4 = checked(num2 - 1);
                    int num5 = 0;
                    while (num5 <= num4)
                    {
                        short num6 = checked((short)unchecked(checked((int)unchecked((short)((int)buffer[offset] | (int)(short)((int)buffer[checked(offset + 1)] << 8))) * this._volume) / 100));
                        buffer[offset] = checked((byte)((int)num6 & (int)byte.MaxValue));
                        buffer[checked(offset + 1)] = checked((byte)((int)unchecked((short)((int)num6 >> 8)) & (int)byte.MaxValue));
                        checked { offset += 2; }
                        checked { ++num5; }
                    }
                    num1 = num3;
                }
            }
            else
            {
                int count2 = Math.Min(checked(this.header.Length - this.headerOffset), count);
                Buffer.BlockCopy((Array)this.header, this.headerOffset, (Array)buffer, offset, count2);
                // ISSUE: variable of a reference type
                int&local;
                // ISSUE: explicit reference operation
                int num7 = checked(^(local = ref this.headerOffset) + count2);
                local = num7;
                if (this.headerOffset == this.header.Length)
                    this.header = (byte[])null;
                num1 = count2;
            }
            return num1;
        }

        public override void SetLength(long value)
        {
            this.CheckDisposed();
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            this.CheckDisposed();
            throw new NotSupportedException();
        }

        public override void Flush()
        {
            this.CheckDisposed();
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.CheckDisposed();
            throw new NotSupportedException();
        }

        private void CheckDisposed()
        {
            if (this.IsClosed)
                throw new ObjectDisposedException(this.GetType().FullName);
        }
    }
}
