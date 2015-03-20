//==================================================================//
/*
    AtomicParsley - config.h

    AtomicParsley is GPL software; you can freely distribute,
    redistribute, modify & use under the terms of the GNU General
    Public License; either version 2 or its successor.

    AtomicParsley is distributed under the GPL "AS IS", without
    any warranty; without the implied warranty of merchantability
    or fitness for either an expressed or implied particular purpose.

    Please see the included GNU General Public License (GPL) for
    your rights and further details; see the file COPYING. If you
    cannot, write to the Free Software Foundation, 59 Temple Place
    Suite 330, Boston, MA 02111-1307, USA.  Or www.fsf.org

    Copyright ©2012
    with contributions from others; see the CREDITS file
                                                                   */
//==================================================================//

#define VERSION "0.9.6"
#define PACKAGE "atomicparsley"
#define PACKAGE_VERSION "0.9.6"
#define PACKAGE_BUGREPORT "http://bitbucket.org/wez/atomicparsley/issues/new/"
#define PACKAGE_NAME "atomicparsley"

#define DARWIN_PLATFORM 1

/* #undef HAVE_MALLOC_H */
/* #undef HAVE_WINDOWS_H */
#define HAVE_SYS_TIME_H 1
#define HAVE_STDDEF_H 1
#define HAVE_STDINT_H 1
#define HAVE_INTTYPES_H 1
#define HAVE_FCNTL_H 1
#define HAVE_SYS_IOCTL_H 1
/* #undef HAVE_LINUX_CDROM_H */
#define HAVE_SYS_MOUNT_H 1
#define HAVE_SYS_PARAM_H 1
#define HAVE_WCHAR_H 1
#define HAVE_SYS_STAT_H 1
#define HAVE_UNISTD_H 1
/* #undef HAVE_IO_H */
#define HAVE_SIGNAL_H 1
#define HAVE_GETOPT_H 1

#define HAVE_FSEEKO 1
#define HAVE_FSETPOS 1
#define HAVE_LROUNDF 1
#define HAVE_MALLOC 1
#define HAVE_MEMCMP 1
#define HAVE_MEMSET 1
#define HAVE_REMOVE 1
#define HAVE_RENAME 1
#define HAVE_SCANF 1
#define HAVE_STRDUP 1
#define HAVE_ERROR 1
#define HAVE_STRFTIME 1
#define HAVE_STRNCASECMP 1
#define HAVE_STRNCMP 1
#define HAVE_STRRCHR 1
#define HAVE_STRSEP 1
#define HAVE_STRSTR 1
#define HAVE_STRTOL 1
#define HAVE_WMEMSET 1
#define HAVE_SNPRINTF 1
/* #undef HAVE__SNPRINTF */
#define HAVE_STRTOUMAX 1

#define HAVE_UINTMAX_T 1
/* #undef HAVE_UINTPTR_T */
#define HAVE_UNSIGNED_LONG_LONG_INT 1

#ifdef HAVE__SNPRINTF
#ifndef snprintf
#define snprintf _snprintf
#endif
#endif
