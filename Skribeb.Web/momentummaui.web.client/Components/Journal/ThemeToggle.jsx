import React from "react";
import { Sun, Moon } from "lucide-react";
import { Button } from "@/components/ui/button";
import { motion } from "framer-motion";

export default function ThemeToggle({ theme, onToggle }) {
    return (
        <Button
            variant="outline"
            size="icon"
            onClick={onToggle}
            className="rounded-full bg-card border-border shadow-sm"
        >
            <motion.div

                initial={{ rotate: 0, scale: 0 }}
                animate={{ rotate: 360, scale: 1 }}
                transition={{ duration: 0.3 }}
                key={theme}

            >
                {theme === "light" ? (
                    <Moon className="w-5 h-5 text-foreground" />
                ) : (
                    <Sun className="w-5 h-5 text-foreground" />
                )}
            </motion.div>
        </Button>
    );
}