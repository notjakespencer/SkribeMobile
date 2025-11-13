import React, { useState } from "react";
import { base44 } from "@/api/base44Client";
import { useQuery } from "@tanstack/react-query";
import { Button } from "@/components/ui/button";
import { ChevronLeft, ChevronRight } from "lucide-react";
import { format, addMonths, subMonths } from "date-fns";
import { AnimatePresence } from "framer-motion";

import CalendarGrid from "../components/calendar/CalendarGrid";
import EntryModal from "../components/calendar/EntryModal";

export default function HistoryPage() {
    const [currentMonth, setCurrentMonth] = useState(new Date());
    const [selectedEntry, setSelectedEntry] = useState(null);

    const { data: entries, isLoading } = useQuery({
        queryKey: ["journalEntries"],
        queryFn: () => base44.entities.JournalEntry.list("-date"),
        initialData: [],
    });

    const { data: user } = useQuery({
        queryKey: ["user"],
        queryFn: () => base44.auth.me(),
    });

    const goToPreviousMonth = () => {
        setCurrentMonth(subMonths(currentMonth, 1));
    };

    const goToNextMonth = () => {
        setCurrentMonth(addMonths(currentMonth, 1));
    };

    const totalEntries = entries.length;
    const moodCounts = entries.reduce((acc, entry) => {
        acc[entry.mood] = (acc[entry.mood] || 0) + 1;
        return acc;
    }, {});

    return (
        <div className="min-h-screen md:ml-20 p-4 md:p-8 bg-background">
            <div className="max-w-5xl mx-auto">
                {/* Header */}
                <div className="mb-8">
                    <h1 className="text-3xl font-bold text-foreground mb-2">
                        Your Journal History
                    </h1>
                    <p className="text-muted-foreground">
                        Reflect on your journey and track your moods over time
                    </p>
                </div>

                {/* Stats Cards */}
                <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-8">
                    <div className="bg-card rounded-2xl p-4 border border-border">
                        <div className="text-2xl font-bold text-foreground">{totalEntries}</div>
                        <div className="text-sm text-muted-foreground">Total Entries</div>
                    </div>
                    <div className="bg-card rounded-2xl p-4 border border-border">
                        <div className="text-2xl font-bold text-foreground">
                            {user?.current_streak || 0}
                        </div>
                        <div className="text-sm text-muted-foreground">Current Streak</div>
                    </div>
                    <div className="bg-card rounded-2xl p-4 border border-border">
                        <div className="text-2xl font-bold text-foreground">
                            {user?.longest_streak || 0}
                        </div>
                        <div className="text-sm text-muted-foreground">Longest Streak</div>
                    </div>
                    <div className="bg-card rounded-2xl p-4 border border-border">
                        <div className="text-2xl font-bold text-foreground">
                            Level {user?.level || 1}
                        </div>
                        <div className="text-sm text-muted-foreground">Current Level</div>
                    </div>
                </div>

                {/* Month Navigation */}
                <div className="flex items-center justify-between mb-6">
                    <Button
                        variant="outline"
                        size="icon"
                        onClick={goToPreviousMonth}
                        className="rounded-full"
                    >
                        <ChevronLeft className="w-5 h-5" />
                    </Button>
                    <h2 className="text-xl font-bold text-foreground">
                        {format(currentMonth, "MMMM yyyy")}
                    </h2>
                    <Button
                        variant="outline"
                        size="icon"
                        onClick={goToNextMonth}
                        className="rounded-full"
                        disabled={currentMonth >= new Date()}
                    >
                        <ChevronRight className="w-5 h-5" />
                    </Button>
                </div>

                {/* Calendar */}
                {isLoading ? (
                    <div className="flex items-center justify-center py-20">
                        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary" />
                    </div>
                ) : (
                    <CalendarGrid
                        currentMonth={currentMonth}
                        entries={entries}
                        onDateClick={setSelectedEntry}
                    />
                )}

                {/* Mood Legend */}
                <div className="mt-8 bg-card rounded-2xl p-6 border border-border">
                    <h3 className="font-semibold mb-4 text-foreground">Mood Legend</h3>
                    <div className="grid grid-cols-2 md:grid-cols-5 gap-3">
                        <div className="flex items-center gap-2">
                            <div className="w-4 h-4 rounded-full bg-gradient-to-br from-green-400 to-emerald-500" />
                            <span className="text-sm text-foreground">Amazing ({moodCounts.amazing || 0})</span>
                        </div>
                        <div className="flex items-center gap-2">
                            <div className="w-4 h-4 rounded-full bg-gradient-to-br from-lime-400 to-green-500" />
                            <span className="text-sm text-foreground">Good ({moodCounts.good || 0})</span>
                        </div>
                        <div className="flex items-center gap-2">
                            <div className="w-4 h-4 rounded-full bg-gradient-to-br from-yellow-400 to-orange-400" />
                            <span className="text-sm text-foreground">Okay ({moodCounts.okay || 0})</span>
                        </div>
                        <div className="flex items-center gap-2">
                            <div className="w-4 h-4 rounded-full bg-gradient-to-br from-orange-400 to-red-400" />
                            <span className="text-sm text-foreground">Tough ({moodCounts.tough || 0})</span>
                        </div>
                        <div className="flex items-center gap-2">
                            <div className="w-4 h-4 rounded-full bg-gradient-to-br from-red-400 to-rose-500" />
                            <span className="text-sm text-foreground">Difficult ({moodCounts.difficult || 0})</span>
                        </div>
                    </div>
                </div>

                {/* Entry Modal */}
                <AnimatePresence>
                    {selectedEntry && (
                        <EntryModal
                            entry={selectedEntry}
                            onClose={() => setSelectedEntry(null)}
                        />
                    )}
                </AnimatePresence>
            </div>
        </div>
    );
}