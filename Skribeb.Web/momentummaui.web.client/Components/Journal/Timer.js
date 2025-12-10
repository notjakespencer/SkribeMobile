import React from "react";
import { motion } from "framer-motion";
import { Clock } from "lucide-react";

export default function Timer({ isActive, timeLeft }) {
    const minutes = Math.floor(timeLeft / 60);
    const seconds = timeLeft % 60;
    const progress = (timeLeft / 120) * 100;

    return (
        <div className="flex flex-col items-center gap-4">
            <div className="relative w-32 h-32">
                {/* Background circle */}
                <svg className="w-full h-full transform -rotate-90">
                    <circle
                        cx="64"
                        cy="64"
                        r="56"
                        fill="none"
                        stroke="currentColor"
                        strokeWidth="8"
                        className="text-secondary"
                    />
                    {/* Progress circle */}
                    <motion.circle

                        initial={false} // Prevent animation on initial render
                        animate={{ strokeDashoffset: 352 - (352 * progress) / 100 }}
                        transition={{ duration: 0.5, ease: "linear" }}

                        cx="64"
                        cy="64"
                        r="56"
                        fill="none"
                        stroke="currentColor"
                        strokeWidth="8"
                        strokeLinecap="round"
                        strokeDasharray="352"
                        className="text-primary"
                    />
                </svg>

                {/* Time display */}
                <div className="absolute inset-0 flex items-center justify-center">
                    <div className="text-center">
                        <div className="text-3xl font-bold text-foreground">
                            {minutes}:{seconds.toString().padStart(2, "0")}
                        </div>
                        <div className="text-xs text-muted-foreground mt-1">
                            {isActive ? "remaining" : "ready"}
                        </div>
                    </div>
                </div>
            </div>

            {!isActive && (
                <motion.div

                    initial={{ opacity: 0, y: 10 }}
                    animate={{ opacity: 1, y: 0 }}

                    className="flex items-center gap-2 text-sm text-muted-foreground"
                >
                    <Clock className="w-4 h-4" />
                    <span>Start typing to begin</span>
                </motion.div>
            )}
        </div>
    );
}