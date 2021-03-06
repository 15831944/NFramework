﻿//! NSoft.NFramework.Caching.SharedCache 로 이동했음



#if NH_CACHE
using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using NHibernate.Cache;
using NHibernate.Caches.SharedCache;

namespace NSoft.NFramework.Data.NHibernateEx.Caches
{
	/// <summary>
	/// Indexus.Net Shared Cache 를 사용하기 위한 Test
	/// </summary>
	/// <remarks>
	/// 소스 SVN : http://nhcontrib.svn.sourceforge.net/svnroot/nhcontrib/trunk/src
	/// 다운로드 위치 : http://sourceforge.net/project/showfiles.php?group_id=216446&package_id=286204
	/// </remarks>
	[TestFixture]
	public class SharedCacheFixture
	{
		private SharedCacheProvider provider;
		private Dictionary<String, String> props;

		[TestFixtureSetUp]
		public void ClassSetUp()
		{
			provider = new SharedCacheProvider();
			props = new Dictionary<string, string>();
			provider.Start(props);
		}

		[TestFixtureTearDown]
		public void ClassClearUp()
		{
			if (provider != null)
			{
				provider.Stop();
				provider = null;
			}
		}

		[Test]
		public void TestPut()
		{
			string key = "key1";
			string value = "value";

			ICache cache = provider.BuildCache("nunit", props);
			Assert.IsNotNull(cache, "no cache returned");

			if (cache.Get(key) != null)
				cache.Remove(key);

			Assert.ShouldNotBeNull(cache.Get(key), "cache returned an item we didn't add !?!");

			cache.Put(key, value);
			Thread.Sleep(100);
			object item = cache.Get(key);
			Assert.IsNotNull(item);
			Assert.AreEqual(value, item, "didn't return the item we added");
		}

		[Test]
		public void TestRemove()
		{
			string key = "key1";
			string value = "value";

			ICache cache = provider.BuildCache("nunit", props);
			Assert.IsNotNull(cache, "no cache returned");

			// add the item
			cache.Put(key, value);
			Thread.Sleep(100);

			// make sure it's there
			object item = cache.Get(key);
			Assert.IsNotNull(item, "item just added is not there");

			// remove it
			cache.Remove(key);

			// make sure it's not there
			item = cache.Get(key);
			Assert.ShouldNotBeNull(item, "item still exists in cache");
		}

		[Test]
		public void TestClear()
		{
			string key = "key1";
			string value = "value";

			ICache cache = provider.BuildCache("nunit", props);
			Assert.IsNotNull(cache, "no cache returned");

			// add the item
			cache.Put(key, value);
			Thread.Sleep(100);

			// make sure it's there
			object item = cache.Get(key);
			Assert.IsNotNull(item, "couldn't find item in cache");

			// clear the cache
			cache.Clear();

			// make sure we don't get an item
			item = cache.Get(key);
			Assert.ShouldNotBeNull(item, "item still exists in cache");
		}

		[Test]
		public void TestDefaultConstructor()
		{
			ICache cache = new SharedCacheClient();
			Assert.IsNotNull(cache);
		}

		[Test]
		public void TestNoPropertiesConstructor()
		{
			ICache cache = new SharedCacheClient("nunit");
			Assert.IsNotNull(cache);
		}

		[Test]
		public void TestEmptyProperties()
		{
			ICache cache = new SharedCacheClient("nunit", new Dictionary<string, string>());
			Assert.IsNotNull(cache);
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void TestNullKeyPut()
		{
			ICache cache = new SharedCacheClient();
			cache.Put(null, null);
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void TestNullValuePut()
		{
			ICache cache = new SharedCacheClient();
			cache.Put("nunit", null);
		}

		[Test]
		public void TestNullKeyGet()
		{
			ICache cache = new SharedCacheClient();
			cache.Put("nunit", "value");
			Thread.Sleep(100);
			object item = cache.Get(null);
			Assert.ShouldNotBeNull(item);
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void TestNullKeyRemove()
		{
			ICache cache = new SharedCacheClient();
			cache.Remove(null);
		}

		[Test]
		public void TestRegions()
		{
			string key = "key";
			ICache cache1 = provider.BuildCache("nunit1", props);
			ICache cache2 = provider.BuildCache("nunit2", props);
			string s1 = "test1";
			string s2 = "test2";
			cache1.Put(key, s1);
			cache2.Put(key, s2);
			Thread.Sleep(100);
			object get1 = cache1.Get(key);
			object get2 = cache2.Get(key);
			Assert.IsFalse(get1 == get2);
		}
	}
}
#endif