import React from "react";
import { motion } from "framer-motion";
import { Button } from "@/components/ui/button";

const moods = [
    { value: "amazing", emoji: "🤩", label: "Amazing", color: "from-green-400 to-emerald-500" },
    { value: "good", emoji: "😊", label: "Good", color: "from-lime-400 to-green-500" },
    { value: "okay", emoji: "😐", label: "Okay", color: "from-yellow-400 to-orange-400" },
    { value: "tough", emoji: "😔", label: "Tough", color: "from-orange-400 to-red-400" },
    { value: "difficult", emoji: "😢", label: "Difficult", color: "from-red-400 to-rose-500" },
];

export default function MoodSelector({ onSelect }) {
    return (
        <motion.div

            initial={{ opacity: 0, scale: 0.9 }}
            animate={{ opacity: 1, scale: 1 }}

            className="fixed inset-0 bg-background/80 backdrop-blur-sm z-50 flex items-center justify-center p-4"
        >
            <motion.div

                initial={{ y: 50 }}
                animate={{ y: 0 }}

                className="bg-card rounded-3xl p-8 max-w-md w-full border border-border shadow-2xl"
            >
                <h2 className="text-2xl font-bold text-center mb-2 text-foreground">
                    How are you feeling today?
                </h2>
                <p className="text-center text-muted-foreground mb-6">
                    Choose the mood that best describes your day
                </p>

                <div className="space-y-3">
                    {moods.map((mood, index) => (
                        <motion.div
                            key={mood.value}

                            initial={{ opacity: 0, x: -20 }}
                            animate={{ opacity: 1, x: 0 }}
                            transition={{ delay: index * 0.1 }}

                        >
                            <Button
                                onClick={() => onSelect(mood.value)}
                                className={`w-full h-auto py-4 px-6 bg-gradient-to-r ${mood.color} hover:scale-105 transition-transform duration-200 text-white border-0`}
                            >
                                <span className="text-3xl mr-3">{mood.emoji}</span>
                                <span className="text-lg font-semibold">{mood.label}</span>
                            </Button>
                        </motion.div>
                    ))}
                </div>
            </motion.div>
        </motion.div>
    );
}