// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Research.SEAL;
using System;
using System.IO;
using System.Text;

namespace SEALNetTest
{
    [TestClass]
    public class SerializationTests
    {
        [TestMethod]
        public void SEALHeaderTest()
        {
            Assert.AreEqual(Serialization.SEALHeader.Bytes(), 16);

            Serialization.SEALHeader header = new Serialization.SEALHeader();
            Serialization.SEALHeader loaded = new Serialization.SEALHeader();
            using (MemoryStream mem = new MemoryStream())
            {
                header.ComprMode = ComprModeType.Deflate;
                header.Size = 256;
                Assert.IsTrue(Serialization.IsValidHeader(header));

                Serialization.SaveHeader(header, mem);
                mem.Seek(offset: 0, loc: SeekOrigin.Begin);
                Serialization.LoadHeader(mem, loaded);

                Assert.AreEqual(loaded.Magic, header.Magic);
                Assert.AreEqual(loaded.VersionMajor, header.VersionMajor);
                Assert.AreEqual(loaded.VersionMinor, header.VersionMinor);
                Assert.AreEqual(loaded.ComprMode, header.ComprMode);
                Assert.AreEqual(loaded.ZeroByte, header.ZeroByte);
                Assert.AreEqual(loaded.Reserved, header.Reserved);
                Assert.AreEqual(loaded.Size, header.Size);
            }
        }

        [TestMethod]
        public void ExceptionsTest()
        {
            SEALContext context = GlobalContext.BFVContext;
            Ciphertext cipher = new Ciphertext();

            using (MemoryStream mem = new MemoryStream())
            {
                KeyGenerator keygen = new KeyGenerator(context);
                Encryptor encryptor = new Encryptor(context, keygen.PublicKey);
                Plaintext plain = new Plaintext("2x^3 + 4x^2 + 5x^1 + 6");
                encryptor.Encrypt(plain, cipher);
                cipher.Save(mem);
                mem.Seek(offset: 8, loc: SeekOrigin.Begin);
                BinaryWriter writer = new BinaryWriter(mem, Encoding.UTF8, true);
                writer.Write((ulong)0x80000000);

                mem.Seek(offset: 0, loc: SeekOrigin.Begin);
                Utilities.AssertThrows<InvalidOperationException>(() => cipher.Load(context, mem));
            }
        }
    }
}