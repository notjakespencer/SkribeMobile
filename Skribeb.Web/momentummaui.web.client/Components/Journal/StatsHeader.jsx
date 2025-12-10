import React from "react";
import { Flame, Zap } from "lucide-react";
import { motion } from "framer-motion";

export default function StatsHeader({ streak, level, xp }) {
    // XP required to reach the current level
    const xpAtStartOfLevel = 100 * (level - 1) * (level) / 2;
    // XP gained since the current level started
    const xpIntoCurrentLevel = xp - xpAtStartOfLevel;
    // XP needed to advance to the next level
    const xpNeededForNextLevel = level * 100;

    const xpProgress = (xpIntoCurrentLevel / xpNeededForNextLevel) * 100;

    return (
        <div className="flex items-center justify-between gap-4">
            {/* Streak */}
            <motion.div

                initial={{ scale: 0 }}
                animate={{ scale: 1 }}

                className="flex items-center gap-2 bg-card rounded-2xl px-4 py-2 border border-border shadow-sm"
            >
                <div className="relative">
                    <Flame
                        className={`w-6 h-6 ${streak > 0 ? "text-orange-500" : "text-muted-foreground"
                            }`}
                    />
                    {streak > 0 && (
                        <motion.div

                            animate={{
                                scale: [1, 1.2, 1],
                                opacity: [1, 0.8, 1],
                            }}
                            transition={{
                                duration: 2,
                                repeat: Infinity,
                            }}

                            className="absolute inset-0 bg-orange-400 rounded-full blur-md opacity-50"
                        />
                    )}
                </div>
                <div>
                    <div className="text-sm font-bold text-foreground">{streak}</div>
                    <div className="text-xs text-muted-foreground">day streak</div>
                </div>
            </motion.div>

            {/* Level & XP */}
            <motion.div

                initial={{ scale: 0 }}
                animate={{ scale: 1 }}
                transition={{ delay: 0.1 }}

                className="flex-1 bg-card rounded-2xl px-4 py-2 border border-border shadow-sm"
            >
                <div className="flex items-center gap-2 mb-1">
                    <Zap className="w-4 h-4 text-primary" />
                    <span className="text-xs font-medium text-muted-foreground">
                        Level {level}
                    </span>
                </div>
                <div className="relative h-2 bg-secondary rounded-full overflow-hidden">
                    <motion.div

                        initial={{ width: 0 }}
                        animate={{ width: `${xpProgress}%` }}
                        transition={{ duration: 0.5, delay: 0.2 }}

                        className="absolute inset-y-0 left-0 bg-gradient-to-r from-primary to-purple-500 rounded-full"
                    />
                </div>
                <div className="text-xs text-muted-foreground mt-1">
                    {Math.floor(xpIntoCurrentLevel)} / {xpNeededForNextLevel} XP
                </div>
            </motion.div>
        </div>
    );
}