namespace AppointmentSystem.Business.Extensions
{
    using AppointmentSystem.Data.Entities;
    using System.Collections.Generic;
    using System.Linq;

    public static class SlotExtensions
    {
        /// <summary>
        /// Determines if a slot overlaps with another slot for the same SalesManager.
        /// </summary>
        public static bool OverlapWith(this Slot slot, Slot other)
        {
            return slot.SalesManagerId == other.SalesManagerId &&
                   slot.StartDate < other.EndDate &&
                   slot.EndDate > other.StartDate;
        }

        /// <summary>
        /// Filters out overlapping available slots against booked slots.
        /// </summary>
        public static List<Slot> RemoveOverlappingSlots(this List<Slot> availableSlots, List<Slot> bookedSlots)
        {
            return availableSlots
                .Where(aSlot => !bookedSlots.Any(bSlot => aSlot.OverlapWith(bSlot))) // ✅ Fixed: Renamed method
                .ToList();
        }
    }
}
