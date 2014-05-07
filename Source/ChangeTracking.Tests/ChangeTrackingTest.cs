﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ChangeTracking;
using System.Collections.Generic;
using System.Diagnostics;
using FluentAssertions;
using System.Linq;

namespace ChangeTracking.Tests
{
    [TestClass]
    public class ChangeTrackingTest
    {
        public TestContext TestContext { get; set; }

        private Order GetOrder(int id = 1, string custumerNumber = "Test")
        {
            return new Order
              {
                  Id = 1,
                  CustumerNumber = "Test"
              };
        }

        private IList<Order> GetOrdersIList()
        {
            return Enumerable.Range(0, 10).Select(i =>
                {
                    var order = GetOrder();
                    order.Id = i;
                    return order;
                }).ToList();
        }

        [TestMethod]
        public void AsTrackable_Should_Make_Object_Implement_IChangeTrackable()
        {
            var order = GetOrder();

            Order trackable = order.AsTrackable();

            trackable.Should().BeAssignableTo<IChangeTrackable<Order>>();
        }

        [TestMethod]
        public void When_AsTrackable_CastToIChangeTrackable_Should_Not_Throw_InvalidCastException()
        {
            var order = GetOrder();

            Order trackable = order.AsTrackable();

            trackable.Invoking(o => o.CastToIChangeTrackable()).ShouldNotThrow<InvalidCastException>();
        }

        [TestMethod]
        public void When_Not_AsTrackable_CastToIChangeTrackable_Should_Throw_InvalidCastException()
        {
            var order = GetOrder();

            order.Invoking(o => o.CastToIChangeTrackable()).ShouldThrow<InvalidCastException>();
        }

        [TestMethod]
        public void Change_Property_Should_Raise_Event()
        {
            var order = GetOrder();
            var trackable = order.AsTrackable();
            trackable.MonitorEvents();

            trackable.CustumerNumber = "Test1";

            trackable.ShouldRaise("StatusChanged");
        }

        [TestMethod]
        public void Change_Property_To_Same_Value_Should_Not_Raise_Event()
        {
            var order = GetOrder();
            var trackable = order.AsTrackable();
            trackable.MonitorEvents();

            trackable.CustumerNumber = "Test";

            trackable.ShouldNotRaise("StatusChanged");
        }

        [TestMethod]
        public void Change_Property_From_Null_To_Value_Should_Not_Throw()
        {
            var trackable = new Order { Id = 321, CustumerNumber = null }.AsTrackable();

            trackable.Invoking(o => o.CustumerNumber = "Test").ShouldNotThrow<NullReferenceException>();
        }

        [TestMethod]
        public void GetOriginalValue_Should_Return_Original_Value()
        {
            var order = GetOrder();
            var trackable = order.AsTrackable();

            trackable.CustumerNumber = "Test1";

            trackable.CastToIChangeTrackable().GetOriginalValue(o => o.CustumerNumber).Should().Be("Test");
        }

        [TestMethod]
        public void GetOriginal_Should_Return_Original()
        {
            var order = GetOrder();
            var trackable = order.AsTrackable();

            trackable.Id = 124;
            trackable.CustumerNumber = "Test1";

            var original = trackable.CastToIChangeTrackable().GetOriginal();
            var newOne = GetOrder();
            original.ShouldBeEquivalentTo(newOne);
        }

        [TestMethod]
        public void AsTrackable_On_Collection_Should_Make_Object_Implement_IChangeTrackableCollection()
        {
            var orders = GetOrdersIList();

            ICollection<Order> trackable = orders.AsTrackable();

            trackable.Should().BeAssignableTo<IChangeTrackableCollection<Order>>();
        }

        [TestMethod]
        public void When_AsTrackable_On_Collection_CastToIChangeTrackable_Should_Not_Throw_InvalidCastException()
        {
            var orders = GetOrdersIList();

            IList<Order> trackable = orders.AsTrackable();

            trackable.Invoking(o => o.CastToIChangeTrackableCollection()).ShouldNotThrow<InvalidCastException>();
        }

        [TestMethod]
        public void When_Not_AsTrackable_On_Collection_CastToIChangeTrackable_Should_Throw_InvalidCastException()
        {
            var orders = GetOrdersIList();

            orders.Invoking(o => o.CastToIChangeTrackableCollection()).ShouldThrow<InvalidCastException>();
        }

        [TestMethod]
        public void When_Calling_AsTrackable_On_Collection_Already_Tracking_Should_Throw()
        {
            var orders = GetOrdersIList();

            orders[0] = orders[0].AsTrackable();
            orders[0].CustumerNumber = "Test1";

            orders.Invoking(list => list.AsTrackable()).ShouldThrow<InvalidOperationException>();
        }

        [TestMethod]
        public void When_Calling_AsTrackable_On_Collection_All_Items_Should_Become_Trackable()
        {
            var orders = GetOrdersIList();

            orders.AsTrackable();

            orders.Should().ContainItemsAssignableTo<IChangeTrackable<Order>>();
        }

        [TestMethod]
        public void When_Setting_Status_Should_Be_That_Status()
        {
            var order = GetOrder();

            var trackable = order.AsTrackable(ChangeStatus.Added);

            trackable.CastToIChangeTrackable().ChangeTrackingStatus.Should().Be(ChangeStatus.Added);
        }

        [TestMethod]
        public void When_Status_Added_And_Change_Value_Status_Should_Stil_Be_Added()
        {
            var order = GetOrder();

            var trackable = order.AsTrackable(ChangeStatus.Added);
            trackable.CustumerNumber = "Test1";

            trackable.CastToIChangeTrackable().ChangeTrackingStatus.Should().Be(ChangeStatus.Added);
        }

        [TestMethod]
        public void When_Status_Is_Deleted_And_Change_Value_Should_Throw()
        {
            var order = GetOrder();

            var trackable = order.AsTrackable(ChangeStatus.Deleted);

            trackable.Invoking(o => o.CustumerNumber = "Test1").ShouldThrow<InvalidOperationException>();
        }

        [TestMethod]
        public void When_Adding_To_Colletion_Should_Be_IChangeTracableTrackable()
        {
            var orders = GetOrdersIList();

            var trackable = orders.AsTrackable();
            trackable.Add(new Order { Id = 999999999, CustumerNumber = "Custumer" });

            trackable.Single(o => o.Id == 999999999).Should().BeAssignableTo<IChangeTrackable<Order>>();
        }

        [TestMethod]
        public void When_Adding_To_Colletion_Status_Should_Be_Added()
        {
            var orders = GetOrdersIList();

            var trackable = orders.AsTrackable();
            trackable.Add(new Order { Id = 999999999, CustumerNumber = "Custumer" });

            trackable.Single(o => o.Id == 999999999).CastToIChangeTrackable().ChangeTrackingStatus.Should().Be(ChangeStatus.Added);
        }

        [TestMethod]
        public void When_Adding_To_Colletion_Via_Indexer_Status_Should_Be_Added()
        {
            IList<Order> list = new List<Order>();

            var trackable = list.AsTrackable();
            trackable.Add(new Order { Id = 999999999, CustumerNumber = "Custumer" });

            trackable.Single(o => o.Id == 999999999).CastToIChangeTrackable().ChangeTrackingStatus.Should().Be(ChangeStatus.Added);
        }

        [TestMethod]
        public void When_Deleting_From_Colletion_Status_Should_Be_Deleted()
        {
            var orders = GetOrdersIList();

            var trackable = orders.AsTrackable();
            var first = trackable.First();
            trackable.Remove(first);

            first.CastToIChangeTrackable().ChangeTrackingStatus.Should().Be(ChangeStatus.Deleted);
        }

        [TestMethod]
        public void When_Deleting_From_Colletion_Status_Should_Be_Added_To_DeletedItems()
        {
            var orders = GetOrdersIList();

            var trackable = orders.AsTrackable();
            var first = trackable.First();
            trackable.Remove(first);

            trackable.CastToIChangeTrackableCollection().DeletedItems.Should().HaveCount(1)
                .And.OnlyContain(o => o.Id == first.Id && o.CustumerNumber == first.CustumerNumber);
        }

        [TestMethod]
        public void When_Using_Not_On_IList_Of_T_Or_Collection_Of_T_Should_Throw()
        {
            var orders = GetOrdersIList().ToArray();

            orders.Invoking(o => o.AsTrackable()).ShouldThrow<InvalidOperationException>();
        }

        [TestMethod]
        public void AsTrackable_On_Collection_Should_Make_It_IBindingList()
        {
            var orders = GetOrdersIList();

            var trackable = orders.AsTrackable();

            trackable.Should().BeAssignableTo<System.ComponentModel.IBindingList>();
        }

        [TestMethod]
        public void AsTrackable_On_Collection_AddNew_Should_Raise_ListChanged()
        {
            var orders = GetOrdersIList();

            var trackable = orders.AsTrackable();
            var bindingList = (System.ComponentModel.IBindingList)trackable;

            bindingList.MonitorEvents();
            bindingList.AddNew();

            bindingList.ShouldRaise("ListChanged");
        }

        [TestMethod]
        public void CancelEdit_On_Item_Should_Remove_From_Collection()
        {
            var orders = GetOrdersIList();

            var trackable = orders.AsTrackable();
            var bindingList = (System.ComponentModel.IBindingList)trackable;

            bindingList.AddNew();
            var withAddedCount = bindingList.Count;
            var addedItem = bindingList.Cast<Order>().Single(o => o.CustumerNumber == null);
            var editableObject = (System.ComponentModel.IEditableObject)addedItem;
            editableObject.CancelEdit();

            bindingList.Count.Should().Be(withAddedCount - 1, because: "item was canceled");
        }

        [TestMethod]
        public void CancelEdit_On_Item_Should_Revert_Changes()
        {
            var order = GetOrder();

            var trackable = order.AsTrackable();
            var editableObject = (System.ComponentModel.IEditableObject)trackable;

            editableObject.BeginEdit();
            trackable.CustumerNumber = "Testing";
            editableObject.CancelEdit();

            trackable.CustumerNumber.Should().Be("Test", because: "item was canceled");
        }

        [TestMethod]
        public void CancelEdit_On_Item_After_EndEdit_Should_Not_Revert_Changes()
        {
            var order = GetOrder();

            var trackable = order.AsTrackable();
            var editableObject = (System.ComponentModel.IEditableObject)trackable;

            editableObject.BeginEdit();
            trackable.CustumerNumber = "Testing";
            editableObject.EndEdit();
            editableObject.CancelEdit();

            trackable.CustumerNumber.Should().Be("Testing", because: "item was canceled after calling EndEdit");
        }

        [TestMethod]
        public void With_Out_BeginEdit_CancelEdit_Should_Do_Nothing()
        {
            var order = GetOrder();

            var trackable = order.AsTrackable();
            var editableObject = (System.ComponentModel.IEditableObject)trackable;

            trackable.CustumerNumber = "Testing";
            editableObject.CancelEdit();

            trackable.CustumerNumber.Should().Be("Testing", because: "item was canceled after calling EndEdit");
        }

        [TestMethod]
        public void AsTrackable_On_Collection_Should_Make_It_ICancelAddNew()
        {
            var orders = GetOrdersIList();

            var trackable = orders.AsTrackable();

            trackable.Should().BeAssignableTo<System.ComponentModel.ICancelAddNew>();
        }

        [TestMethod]
        public void AcceptChanges_Should_Status_Be_Unchanged()
        {
            var order = GetOrder();

            var trackable = order.AsTrackable();
            trackable.Id = 963;
            trackable.CustumerNumber = "Testing";
            var intf = trackable.CastToIChangeTrackable();

            var oldChangeStatus = intf.ChangeTrackingStatus;
            intf.RejectChanges();

            oldChangeStatus.Should().Be(ChangeStatus.Changed);
            intf.ChangeTrackingStatus.Should().Be(ChangeStatus.Unchanged);
        }

        [TestMethod]
        public void AcceptChanges_Should_AcceptChanges()
        {
            var order = GetOrder();

            var trackable = order.AsTrackable();
            trackable.Id = 963;
            trackable.CustumerNumber = "Testing";
            var intf = trackable.CastToIChangeTrackable();
            intf.AcceptChanges();

            intf.GetOriginal().ShouldBeEquivalentTo(intf.GetOriginal());
            intf.GetOriginalValue(o => o.Id).Should().Be(963);
        }

        [TestMethod]
        public void RejectChanges_Should_Status_Be_Unchanged()
        {
            var order = GetOrder();

            var trackable = order.AsTrackable();
            trackable.Id = 963;
            trackable.CustumerNumber = "Testing";
            var intf = trackable.CastToIChangeTrackable();
            var oldChangeStatus = intf.ChangeTrackingStatus;
            intf.RejectChanges();

            oldChangeStatus.Should().Be(ChangeStatus.Changed);
            intf.ChangeTrackingStatus.Should().Be(ChangeStatus.Unchanged);
        }

        [TestMethod]
        public void RejectChanges_Should_RejectChanges()
        {
            var order = GetOrder();

            var trackable = order.AsTrackable();
            trackable.Id = 963;
            trackable.CustumerNumber = "Testing";
            var intf = trackable.CastToIChangeTrackable();
            intf.RejectChanges();

            trackable.ShouldBeEquivalentTo(GetOrder());
        }

        [TestMethod]
        public void RejectChanges_Should_AcceptChanges_Only_After_Last_AcceptChanges()
        {
            var order = GetOrder();

            var trackable = order.AsTrackable();
            trackable.Id = 963;
            trackable.CustumerNumber = "Testing";
            var intf = trackable.CastToIChangeTrackable();
            intf.AcceptChanges();

            intf.GetOriginal().ShouldBeEquivalentTo(intf.GetOriginal());
            intf.GetOriginalValue(o => o.Id).Should().Be(963);
        }
    }
}
