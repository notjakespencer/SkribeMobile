import React from "react";
import { motion } from "framer-motion";
import { format } from "date-fns";
import { X } from "lucide-react";
import { Button } from "@/components/ui/button";

const moodEmojis = {
    amazing: "🤩",
    good: "😊",
    okay: "😐",
    tough: "😔",
    difficult: "😢",
};

const moodColors = {
    amazing: "from-green-400 to-emerald-500",
    good: "from-lime-400 to-green-500",
    okay: "from-yellow-400 to-orange-400",
    tough: "from-orange-400 to-red-400",
    difficult: "from-red-400 to-rose-500",
};

export default function EntryModal({ entry, onClose }) {
    if (!entry) return null;

    return (
        <motion.div

            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}

            className="fixed inset-0 bg-background/80 backdrop-blur-sm z-50 flex items-center justify-center p-4"
            onClick={onClose}
        >
            <motion.div

                initial={{ scale: 0.9, y: 20 }}
                animate={{ scale: 1, y: 0 }}
                exit={{ scale: 0.9, y: 20 }}

                className="bg-card rounded-3xl p-8 max-w-2xl w-full border border-border shadow-2xl max-h-[80vh] overflow-y-auto"
                onClick={(e) => e.stopPropagation()}
            >
                {/* Header */}
                <div className="flex items-start justify-between mb-6">
                    <div>
                        <h2 className="text-2xl font-bold text-foreground mb-2">
                            {format(new Date(entry.date), "MMMM d, yyyy")}
                        </h2>
                        <div className={`inline-flex items-center gap-2 px-4 py-2 rounded-full bg-gradient-to-r ${moodColors[entry.mood]} text-white`}>
                            <span className="text-2xl">{moodEmojis[entry.mood]}</span>
                            <span className="font-medium capitalize">{entry.mood}</span>
                        </div>
                    </div>
                    <Button
                        variant="ghost"
                        size="icon"
                        onClick={onClose}
                        className="rounded-full"
                    >
                        <X className="w-5 h-5" />
                    </Button>
                </div>

                {/* Prompt */}
                <div className="bg-primary/10 rounded-2xl p-4 mb-6 border border-primary/20">
                    <p className="text-sm font-medium text-primary mb-2">Prompt</p>
                    <p className="text-foreground font-medium">{entry.prompt}</p>
                </div>

                {/* Response */}
                <div>
                    <p className="text-sm font-medium text-muted-foreground mb-3">Your Response</p>
                    <div className="bg-secondary/50 rounded-2xl p-6">
                        <p className="text-foreground leading-relaxed whitespace-pre-wrap">
                            {entry.response}
                        </p>
                    </div>
                </div>

                {/* Metadata */}
                <div className="mt-6 pt-6 border-t border-border flex items-center justify-between text-sm text-muted-foreground">
                    <span>Completed in {entry.completion_time || 120} seconds</span>
                    <span>{format(new Date(entry.created_date), "h:mm a")}</span>
                </div>
            </motion.div>
        </motion.div>
    );
}