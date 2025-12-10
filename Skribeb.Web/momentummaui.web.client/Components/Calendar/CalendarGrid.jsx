import React from "react";
import { motion } from "framer-motion";
import { format, startOfMonth, endOfMonth, eachDayOfInterval, isSameMonth, isToday, isFuture } from "date-fns";

const moodColors = {
    amazing: "bg-gradient-to-br from-green-400 to-emerald-500",
    good: "bg-gradient-to-br from-lime-400 to-green-500",
    okay: "bg-gradient-to-br from-yellow-400 to-orange-400",
    tough: "bg-gradient-to-br from-orange-400 to-red-400",
    difficult: "bg-gradient-to-br from-red-400 to-rose-500",
};

export default function CalendarGrid({ currentMonth, entries, onDateClick }) {
    const monthStart = startOfMonth(currentMonth);
    const monthEnd = endOfMonth(currentMonth);
    const days = eachDayOfInterval({ start: monthStart, end: monthEnd });

    const getEntryForDate = (date) => {
        const dateStr = format(date, "yyyy-MM-dd");
        return entries.find((entry) => entry.date === dateStr);
    };

    const startDay = monthStart.getDay();

    return (
        <div className="bg-card rounded-3xl p-6 border border-border shadow-lg">
            {/* Weekday headers */}
            <div className="grid grid-cols-7 gap-2 mb-4">
                {["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"].map((day) => (
                    <div
                        key={day}
                        className="text-center text-xs font-medium text-muted-foreground py-2"
                    >
                        {day}
                    </div>
                ))}
            </div>

            {/* Calendar days */}
            <div className="grid grid-cols-7 gap-2">
                {/* Empty cells for days before month starts */}
                {Array.from({ length: startDay }).map((_, index) => (
                    <div key={`empty-${index}`} className="aspect-square" />
                ))}

                {/* Actual days */}
                {days.map((day, index) => {
                    const entry = getEntryForDate(day);
                    const isCurrentDay = isToday(day);
                    const isFutureDay = isFuture(day);

                    return (
                        <motion.button
                            key={day.toISOString()}

                            initial={{ opacity: 0, scale: 0.8 }}
                            animate={{ opacity: 1, scale: 1 }}
                            transition={{ delay: index * 0.01 }}
                            whileHover={{ scale: entry ? 1.1 : 1 }}

                            onClick={() => entry && onDateClick(entry)}
                            disabled={!entry || isFutureDay}
                            className={`aspect-square rounded-xl flex items-center justify-center relative transition-all ${entry
                                    ? `${moodColors[entry.mood]} text-white shadow-md cursor-pointer`
                                    : isFutureDay
                                        ? "bg-secondary/30 text-muted-foreground/30 cursor-not-allowed"
                                        : "bg-secondary text-muted-foreground hover:bg-secondary/70"
                                } ${isCurrentDay ? "ring-2 ring-primary ring-offset-2 ring-offset-background" : ""}`}
                        >
                            <span className="text-sm font-medium">{format(day, "d")}</span>
                            {entry && (
                                <div className="absolute bottom-1 w-1 h-1 bg-white rounded-full" />
                            )}
                        </motion.button>
                    );
                })}
            </div>
        </div>
    );
}